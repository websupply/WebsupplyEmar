using System.Net.WebSockets;
using System.Net;
using System.Text;

namespace WebsupplyEmar.API.Helpers
{
    public class WebSocketClass
    {
        public string Servidor { get; set; }
        public WebSocket webSocket { get; set; }
    }

    public class WebSocketHelper
    {
        private static Dictionary<string, HttpListener> httpListeners = new Dictionary<string, HttpListener>();
        private static List<WebSocketClass> WebSockets = new List<WebSocketClass>();
        private static List<string> Mensagens = new List<string>();
        private static readonly object objFechado = new object();

        // Delegate para o callback
        public delegate void MessageReceivedCallback(string message);

        // Evento para notificar sobre a chegada de mensagens específicas
        public event MessageReceivedCallback OnSpecificMessageReceived;

        // Função para Processar a Mensagem do WebSocket
        private async Task ProcessaMensagem(WebSocket webSocket, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            string Mensagem = Encoding.UTF8.GetString(buffer.Array, 0, buffer.Count).Replace("\0", "");
            
            // Adicione a mensagem à lista de mensagens
            lock (objFechado)
            {
                Mensagens.Add(Mensagem);
            }

            // Verifique se a mensagem atende aos critérios específicos
            if (Mensagem == "Tretas")
            {
                // Chame o callback para a mensagem específica
                OnSpecificMessageReceived?.Invoke(Mensagem);
            }

            // Envie a mensagem para todos os clientes conectados
            await TransmitirMensangem(Mensagem);

            // Example: Echo the received message back to the client
            //await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task TransmitirMensangem(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            // Envie a mensagem para todos os clientes conectados
            foreach (var socket in WebSockets)
            {
                try
                {
                    if (socket.webSocket.State == WebSocketState.Open)
                    {
                        await socket.webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                }
            }
        }

        public async Task<bool> IniciaServidor(string Servidor)
        {
            try
            {
                HttpListener httpListener;

                // Verifica se já existe o servidor em andamento
                if (httpListeners.TryGetValue(Servidor, out httpListener))
                {
                    Console.WriteLine($"Não foi possível iniciar o servidor {Servidor}, pois o mesmo já está em execução.");
                    return false;
                }

                // Caso não tenha, inicia o novo servidor
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(Servidor);
                httpListener.Start();

                // Armazena a listagem de servidores ativos
                httpListeners[Servidor] = httpListener;

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
                WebSockets.Add(new WebSocketClass {
                    Servidor = Servidor,
                    webSocket = webSocket
                });
            }

            Console.WriteLine($"Conexão Iniciado no WebSocket por --- {Servidor}");

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

                    WebSocketReceiveResult result = webSocket.ReceiveAsync(buffer, CancellationToken.None).GetAwaiter().GetResult();

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                        Console.WriteLine($"Mensagem Recebida de {Servidor}: {message}");

                        ProcessaMensagem(webSocket, buffer, CancellationToken.None);
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
                                webSocket = webSocket
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
                            webSocket = webSocket
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
                if (!httpListeners.TryGetValue(Servidor, out httpListener))
                {
                    Console.WriteLine($"Não foi possível localizar o servidor {Servidor}.");
                    return false;
                }

                // Fecha o Servidor
                httpListener.Close();
                
                // Remove o Servidor da Lista
                httpListeners.Remove(Servidor);

                // Limpa os WebSockets conectados do server
                lock(objFechado)
                {
                    foreach (var socket in WebSockets)
                    {
                        if(socket.Servidor == Servidor)
                        {
                            socket.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Conexão com o Servidor {Servidor} encerrada", CancellationToken.None);
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
