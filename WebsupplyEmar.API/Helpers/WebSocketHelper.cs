using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace WebsupplyEmar.API.Helpers
{
    public class WebSocketClass
    {
        public string Chave { get; set; }
        public string Servidor { get; set; }
        public string Host { get; set; }
        public WebSocket Conector { get; set; }
    }

    public class WebSocketHelper
    {
        private static Dictionary<string, HttpListener> Servidores = new Dictionary<string, HttpListener>();
        private static List<WebSocketClass> WebSockets = new List<WebSocketClass>();
        private static List<string> Mensagens = new List<string>();
        private static readonly object objFechado = new object();

        // Delegate para o callback
        public delegate void MensagemRecebidaCallback(string message);

        // Evento para notificar sobre a chegada de mensagens específicas
        public event MensagemRecebidaCallback OnMensagemEspecificaRecebida;

        // Função para Processar a Mensagem do WebSocket
        private async Task ProcessaMensagem(string Servidor, string Mensagem, string Chave)
        {            
            // Adicione a mensagem à lista de mensagens
            lock (objFechado)
            {
                Mensagens.Add(Mensagem);
            }

            // Verifique se a mensagem atende aos critérios específicos
            if (Mensagem == "Tretas")
            {
                // Chame o callback para a mensagem específica
                OnMensagemEspecificaRecebida?.Invoke(Mensagem);
            }

            // Envie a mensagem para todos os clientes conectados
            await TransmitirMensangem(Servidor, Mensagem, Chave);

            // Example: Echo the received message back to the client
            //await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task TransmitirMensangem(string Sevidor, string Mensagem, string Chave)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Mensagem);

            // Verifica se irá mandar mensagem somente para um cliente
            if(Chave != null)
            {
                try
                {
                    WebSocketClass socket = WebSockets.Find(find => find.Chave == Chave);

                    if (socket != null)
                    {
                        if (socket.Conector.State == WebSocketState.Open && Sevidor == socket.Servidor)
                        {
                            await socket.Conector.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                }
            }
            // Envie a mensagem para todos os clientes conectados
            else
            {
                foreach (var socket in WebSockets)
                {
                    try
                    {
                        if (socket.Conector.State == WebSocketState.Open && Sevidor == socket.Servidor)
                        {
                            await socket.Conector.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                    }
                }
            }
        }

        public async Task<bool> IniciaServidor(string Servidor)
        {
            try
            {
                HttpListener httpListener;

                // Verifica se já existe o servidor em andamento
                if (Servidores.TryGetValue(Servidor, out httpListener))
                {
                    Console.WriteLine($"Não foi possível iniciar o servidor {Servidor}, pois o mesmo já está em execução.");
                    return false;
                }

                // Caso não tenha, inicia o novo servidor
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(Servidor);
                httpListener.Start();

                // Armazena a listagem de servidores ativos
                Servidores[Servidor] = httpListener;

                Console.WriteLine($"Servidor WebSocket iniciado em --- {Servidor}");

                while (true)
                {
                    HttpListenerContext context = await httpListener.GetContextAsync();

                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }

                    Task.Run(() => ProcessaRequisicao(context, Servidor));
                    //await ProcessaRequisicao(context, Servidor);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção gerada no servidor WebSocket: {ex.Message}");
                return false;
            }
        }

        private void ProcessaRequisicao(HttpListenerContext context, string Servidor)
        {
            // Adicione esta linha para permitir conexões de qualquer origem
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            HttpListenerWebSocketContext webSocketContext = context.AcceptWebSocketAsync(subProtocol: null).GetAwaiter().GetResult();
            WebSocket webSocket = webSocketContext.WebSocket;

            lock (objFechado)
            {
                WebSocketClass newWebSocket = new WebSocketClass
                {
                    Chave = context.Request.QueryString["Chave"] != null ? context.Request.QueryString["Chave"] : Guid.NewGuid().ToString(),
                    Host = context.Request.RemoteEndPoint.ToString(),
                    Servidor = Servidor,
                    Conector = webSocket
                };

                if (WebSockets.Find(find => find.Chave == newWebSocket.Chave
                        && find.Servidor == newWebSocket.Servidor
                        && find.Host == newWebSocket.Host) == null) {
                    WebSockets.Add(newWebSocket);

                    Console.WriteLine($"Conexão Iniciado no WebSocket por --- {Servidor}");
                };
            }

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

                    WebSocketReceiveResult result = webSocket.ReceiveAsync(buffer, CancellationToken.None).GetAwaiter().GetResult();

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string Mensagem = Encoding.UTF8.GetString(buffer.Array, 0, result.Count).Replace("\0", "");

                        Console.WriteLine($"Mensagem Recebida de {Servidor}: {Mensagem}");

                        ProcessaMensagem(Servidor, Mensagem, null);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"Conexão Encerrada pelo Cliente --- {context.Request.UserHostName}");

                        webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão Encerrada pelo Cliente", CancellationToken.None);

                        lock (objFechado)
                        {
                            WebSockets.Remove(new WebSocketClass
                            {
                                Servidor = Servidor,
                                Conector = webSocket
                            });
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Exceção gerada no WebSocket --- {Servidor}: {ex.Message}");
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão Encerrada pelo Servidor", CancellationToken.None);

                    lock (objFechado)
                    {
                        WebSockets.Remove(new WebSocketClass
                        {
                            Servidor = Servidor,
                            Conector = webSocket
                        });
                    }
                }
            }
        }

        public async Task<bool> FechaServidor(string Servidor)
        {
            try
            {
                HttpListener httpListener;

                // Consulta o Servidor
                if (!Servidores.TryGetValue(Servidor, out httpListener))
                {
                    Console.WriteLine($"Não foi possível localizar o servidor {Servidor}.");
                    return false;
                }

                // Fecha o Servidor
                httpListener.Close();

                // Remove o Servidor da Lista
                Servidores.Remove(Servidor);

                // Limpa os WebSockets conectados do server
                lock(objFechado)
                {
                    foreach (var socket in WebSockets)
                    {
                        if(socket.Servidor == Servidor)
                        {
                            socket.Conector.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Conexão com o Servidor {Servidor} encerrada", CancellationToken.None);
                            WebSockets.Remove(socket);
                        }
                    }
                }

                // Exibe a mensagem
                Console.WriteLine($"Servidor {Servidor} parado com sucesso.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção gerada ao tentar encerrar o servidor WebSocket: {ex.Message}");
                return false;
            }
        }
    }
}
