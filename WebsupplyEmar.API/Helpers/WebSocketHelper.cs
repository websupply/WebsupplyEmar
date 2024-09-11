using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Net.Sockets;
using WebsupplyEmar.Dados.ADO;
using System.Net.NetworkInformation;
using Microsoft.Graph.Models;
using System.Text.Json;
using System;
using Tavis.UriTemplates;

namespace WebsupplyEmar.API.Helpers
{
    // Classe Responsável pela estrutura da mensagem do websocket
    public class WebSocketMensagem
    {
        public string Mensagem { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public dynamic Parametros { get; set; }
    }

    // Classe Responsável Pela Estrutura dos dados do WebSocketHelper
    public class WebSocketClass
    {
        public string Servidor { get; set; }
        public string Chave { get; set; }
        public string Usuario { get; set; }
        public string Host { get; set; }
        public int MinutosExpiracao { get; set; }
        public DateTime DataConexao { get; set; }
        public DateTime DataExpiracao { get; set; }
        public WebSocket Conector { get; set; }
    }

    // Classe Responsável Pelas Informações do Websocket
    public class WebSocketInfo
    {
        public string Host { get; set; }
        public DateTime DataHorarioInicio { get; set; }
        public DateTime DataHorarioFim { get; set; }
        public bool ServidorOnline { get; set; }
    }

    // Classe Responsável por Gerenciar o WebSocketHelper
    public class WebSocketHelper
    {
        private static Dictionary<string, HttpListener> Servidores = new Dictionary<string, HttpListener>();
        private static List<WebSocketClass> WebSockets = new List<WebSocketClass>();
        private static List<WebSocketMensagem> Mensagens = new List<WebSocketMensagem>();
        private static readonly object threadUnica = new object();
        private static bool LogGerado;
        private static string Connection;
        private Timer timerValidadorSessao;

        // Cria a Instância da Classe responsável pela estrutura da mensagem de retorno do websocket
        private static WebSocketMensagem webSocketMensagem = new WebSocketMensagem();

        // Função para Definir a string de Conexão com o Banco de Dados
        public void DefineConexaoBD(string connection)
        {
            Connection = connection;
        }

        // Delegate para o callback
        public delegate void MensagemRecebidaCallback(string message);

        // Evento para notificar sobre a chegada de mensagens específicas
        public event MensagemRecebidaCallback OnMensagemEspecificaRecebida;

        // Função para Processar a Mensagem do WebSocket
        private async Task ProcessaMensagem(string Servidor, WebSocketMensagem webSocketMensagem, string Chave)
        {            
            // Adicione a mensagem à lista de mensagens
            lock (threadUnica)
            {
                if(webSocketMensagem.Tipo == "mensagem")
                {
                    Mensagens.Add(webSocketMensagem);
                }
            }

            // Verifique se a mensagem atende aos critérios específicos
            if (webSocketMensagem.Mensagem == "Tretas")
            {
                // Chame o callback para a mensagem específica
                OnMensagemEspecificaRecebida?.Invoke(webSocketMensagem.Mensagem);
            }

            // Envie a mensagem para todos os clientes conectados
            await TransmitirMensangem(Servidor, webSocketMensagem, Chave);

            // Example: Echo the received message back to the client
            //await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        // Função par Transmitir a Mensagem para os Clientes
        private async Task TransmitirMensangem(string Sevidor, WebSocketMensagem webSocketMensagem, string Chave)
        {
            // Converte a mensagem em string e depois passa pro buffer
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(webSocketMensagem));

            // Verifica se irá mandar mensagem somente para uma sala
            if(Chave != null)
            {
                try
                {
                    foreach (var socket in WebSockets)
                    {
                        try
                        {
                            if (socket.Conector.State == WebSocketState.Open && Sevidor == socket.Servidor && Chave == socket.Chave)
                            {
                                await socket.Conector.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        catch (WebSocketException ex)
                        {
                            WebSocketADO.GERA_LOG(Connection, $"Exceção gerada do WebSocket: {ex.Message}");
                            Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    WebSocketADO.GERA_LOG(Connection, $"Exceção gerada do WebSocket: {ex.Message}");
                    Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                }
            }
            // Envie a mensagem para todos os clientes conectados do servidor
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
                        WebSocketADO.GERA_LOG(Connection, $"Exceção gerada do WebSocket: {ex.Message}");
                        Console.WriteLine($"Exceção gerada do WebSocket: {ex.Message}");
                    }
                }
            }
        }

        // Função para Iniciar o Servidor
        public async Task<bool> IniciaServidor(string Servidor)
        {
            try
            {
                HttpListener httpListener;

                // Verifica se já existe o servidor em andamento
                if (Servidores.TryGetValue(Servidor, out httpListener))
                {
                    WebSocketADO.GERA_LOG(Connection, $"Não foi possível iniciar o servidor {Servidor}, pois o mesmo já está em execução.");
                    Console.WriteLine($"Não foi possível iniciar o servidor {Servidor}, pois o mesmo já está em execução.");
                    return false;
                }

                // Caso não tenha, inicia o novo servidor
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(Servidor);
                httpListener.Start();

                // Armazena a listagem de servidores ativos
                Servidores[Servidor] = httpListener;

                WebSocketADO.GERA_LOG(Connection, $"Servidor WebSocket iniciado em --- {Servidor}");
                Console.WriteLine($"Servidor WebSocket iniciado em --- {Servidor}");

                // Inicia o Timer do Validador de Sessão
                timerValidadorSessao = new Timer(ValidaSessaoCliente, null, 5000, 5000);

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
                WebSocketADO.GERA_LOG(Connection, $"Exceção gerada no servidor WebSocket: {ex.Message}");
                Console.WriteLine($"Exceção gerada no servidor WebSocket: {ex.Message}");
                return false;
            }
        }

        // Função para Processar a Requisição feita ao Servidor
        private void ProcessaRequisicao(HttpListenerContext context, string Servidor)
        {
            // Adicione esta linha para permitir conexões de qualquer origem
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            WebSocket Conector = context.AcceptWebSocketAsync(subProtocol: null).GetAwaiter().GetResult().WebSocket;
            WebSocketClass clienteWebSocket = new WebSocketClass();

            // Rever a necessidade deste trecho !!!!!!!!!!!!!!!!!!!!
            lock (threadUnica)
            {
                // Verifica se o Cliente já foi criado e ja consta na listagem de clientes
                if (WebSockets.Find(find => find.Chave == context.Request.QueryString["Chave"]
                        && find.Servidor == Servidor
                        && find.Host == context.Request.RemoteEndPoint.ToString()) == null)
                {
                    // Cria a Estrutura do Novo Cliente
                    clienteWebSocket = new WebSocketClass
                    {
                        Servidor = Servidor,
                        Chave = context.Request.QueryString["Chave"] != null ? context.Request.QueryString["Chave"] : Guid.NewGuid().ToString(),
                        Usuario = context.Request.QueryString["Usuario"] != null ? context.Request.QueryString["Usuario"] : Guid.NewGuid().ToString(),
                        Host = context.Request.RemoteEndPoint.ToString(),
                        MinutosExpiracao = context.Request.QueryString["MinutosExpiracao"] != null && int.TryParse(context.Request.QueryString["MinutosExpiracao"], out _) ? int.Parse(context.Request.QueryString["MinutosExpiracao"]) : 5,
                        DataConexao = DateTime.Now,
                        Conector = Conector
                    };

                    // Insere a Data da Expiracão da Conexão do Cliente
                    clienteWebSocket.DataExpiracao = DateTime.Now.AddMinutes(clienteWebSocket.MinutosExpiracao);

                    // Dispara a Mensagem de Notificação para o Servidor/Sala que um novo cliente está conectado
                    webSocketMensagem = new WebSocketMensagem();

                    webSocketMensagem.Mensagem = $"Usuário {clienteWebSocket.Usuario} conectou na sala";
                    webSocketMensagem.Tipo = "notificacao-conexao";
                    webSocketMensagem.Data = DateTime.Now;
                    webSocketMensagem.Parametros = new
                    {
                        Servidor = clienteWebSocket.Servidor,
                        Usuario = clienteWebSocket.Usuario,
                        Chave = clienteWebSocket.Chave
                    };

                    ProcessaMensagem(clienteWebSocket.Servidor, webSocketMensagem, clienteWebSocket.Chave);

                    // Adiciona o Novo Cliente a Lista de Clientes Conectados
                    WebSockets.Add(clienteWebSocket);

                    WebSocketADO.GERA_LOG(Connection, $"Conexão Iniciado no WebSocket ({Servidor}) por --- {clienteWebSocket.Host}");
                    Console.WriteLine($"Conexão Iniciado no WebSocket ({Servidor}) por --- {clienteWebSocket.Host}");
                }
                else
                {
                    // Recupera o WebSocket da Lista de Clientes
                    clienteWebSocket = WebSockets.Find(websocket => websocket.Conector == Conector);
                };
            }

            try
            {
                while (clienteWebSocket.Conector.State == WebSocketState.Open)
                {
                    // Cria o Buffer para receber a mensagem do websocket
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

                    // Recebe a mensagem do websocket
                    WebSocketReceiveResult result = clienteWebSocket.Conector.ReceiveAsync(buffer, CancellationToken.None).GetAwaiter().GetResult();

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Verifica se o evento de mensagem é referente ao cliente digitando
                        bool ClienteDigitando = false;
                        
                        try
                        {
                            string notificacaoCliente = JsonDocument.Parse(Encoding.UTF8.GetString(buffer.Array, 0, result.Count).Replace("\0", "")).RootElement.GetProperty("Tipo").GetString();
                            
                            if(notificacaoCliente == "digitando")
                            {
                                ClienteDigitando = true;
                            }
                            
                        }
                        catch(Exception ex)
                        {
                            ClienteDigitando = false;
                        }

                        // Verifica se irá enviar a mensagem que o cliente enviou ou se ira enviar uma notificação informando que o cliente está digitando
                        if (ClienteDigitando)
                        {
                            // Estrutura e envia a mensagem para o Servidor/Cliente recebida do Cliente atual
                            webSocketMensagem = new WebSocketMensagem();

                            webSocketMensagem.Mensagem = "";
                            webSocketMensagem.Tipo = "digitando";
                            webSocketMensagem.Data = DateTime.Now;
                            webSocketMensagem.Parametros = new
                            {
                                Servidor = Servidor,
                                Usuario = context.Request.QueryString["Usuario"],
                                Chave = context.Request.QueryString["Chave"]
                            };

                            ProcessaMensagem(Servidor, webSocketMensagem, context.Request.QueryString["Chave"]);
                        }
                        else
                        {
                            // Atualiza o Tempo de Expiração do Cliente
                            clienteWebSocket.DataExpiracao = clienteWebSocket.DataExpiracao.AddMinutes(clienteWebSocket.MinutosExpiracao);

                            // Atualiza o Cliente WebSocket na Lista de Clientes
                            WebSockets[WebSockets.FindIndex(websocket => websocket == clienteWebSocket)].DataExpiracao = clienteWebSocket.DataExpiracao;

                            // Estrutura e envia a mensagem para o Servidor/Cliente recebida do Cliente atual
                            webSocketMensagem = new WebSocketMensagem();

                            webSocketMensagem.Mensagem = Encoding.UTF8.GetString(buffer.Array, 0, result.Count).Replace("\0", "");
                            webSocketMensagem.Tipo = "mensagem";
                            webSocketMensagem.Data = DateTime.Now;
                            webSocketMensagem.Parametros = new
                            {
                                Servidor = Servidor,
                                Usuario = context.Request.QueryString["Usuario"],
                                Chave = context.Request.QueryString["Chave"]
                            };

                            ProcessaMensagem(Servidor, webSocketMensagem, context.Request.QueryString["Chave"]);

                            // Gera o Log no Banco de Dados
                            WebSocketADO.GERA_LOG(Connection, $"Mensagem Recebida de {Servidor}: {webSocketMensagem.Mensagem}");
                            Console.WriteLine($"Mensagem Recebida de {Servidor}: {webSocketMensagem.Mensagem}");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Envia a Mensagem para Servidor/ Sala que o cliente ira desconectar
                        webSocketMensagem = new WebSocketMensagem();

                        webSocketMensagem.Mensagem = $"Usuário {context.Request.QueryString["Usuario"]} desconectou";
                        webSocketMensagem.Tipo = "notificacao-desconexao";
                        webSocketMensagem.Data = DateTime.Now;
                        webSocketMensagem.Parametros = new
                        {
                            Servidor = Servidor,
                            Usuario = context.Request.QueryString["Usuario"],
                            Chave = context.Request.QueryString["Chave"]
                        };

                        ProcessaMensagem(Servidor, webSocketMensagem, context.Request.QueryString["Chave"]);

                        // Gera o Log no Banco de Dados
                        WebSocketADO.GERA_LOG(Connection, $"Conexão Encerrada pelo Cliente --- {context.Request.RemoteEndPoint}");
                        Console.WriteLine($"Conexão Encerrada pelo Cliente --- {context.Request.RemoteEndPoint}");

                        // Desconecta o Cliente do Servidor/Sala
                        clienteWebSocket.Conector.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão Encerrada pelo Cliente", CancellationToken.None);

                        // Remove o Cliente da Lista de Clientes Conectados
                        lock (threadUnica)
                        {
                            WebSockets.Remove(WebSockets.Find(websocket => websocket.Host == context.Request.RemoteEndPoint.ToString()));
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                WebSocketADO.GERA_LOG(Connection, $"Exceção gerada no WebSocket --- {Servidor}: {ex.Message}");
                Console.WriteLine($"Exceção gerada no WebSocket --- {Servidor}: {ex.Message}");
            }
            finally
            {
                // Encerra a Conexão do Cliente se o Servidor for desconectado
                if (clienteWebSocket.Conector.State == WebSocketState.Open)
                {
                    // Envia a Mensagem para Servidor/Sala que o cliente ira desconectar
                    webSocketMensagem = new WebSocketMensagem();

                    webSocketMensagem.Mensagem = $"Usuário {context.Request.QueryString["Usuario"]} foi desconectado";
                    webSocketMensagem.Tipo = "notificacao-desconexao";
                    webSocketMensagem.Data = DateTime.Now;

                    ProcessaMensagem(Servidor, webSocketMensagem, context.Request.QueryString["Chave"]);

                    // Desconecta o Cliente do Servidor/Sala
                    clienteWebSocket.Conector.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão Encerrada pelo Servidor", CancellationToken.None);
                    
                    // Remove o Cliente da Lista de Clientes Conectados
                    lock (threadUnica)
                    {
                        WebSockets.Remove(WebSockets.Find(websocket => websocket.Host == context.Request.RemoteEndPoint.ToString()));
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
                    WebSocketADO.GERA_LOG(Connection, $"Não foi possível localizar o servidor {Servidor}.");
                    Console.WriteLine($"Não foi possível localizar o servidor {Servidor}.");
                    return false;
                }

                // Envia a Mensagem para Servidor/Sala que o cliente ira desconectar
                webSocketMensagem = new WebSocketMensagem();

                webSocketMensagem.Mensagem = "Você foi desconectado do Servidor pois o mesmo foi encerrado";
                webSocketMensagem.Tipo = "notificacao-desconexao";
                webSocketMensagem.Data = DateTime.Now;

                await ProcessaMensagem(Servidor, webSocketMensagem, null);

                // Encerra a Conexão dos Clientes com o servidor que será fechado
                foreach (var socket in WebSockets.FindAll(websocket => websocket.Servidor == Servidor))
                {
                    socket.Conector.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Conexão com o Servidor {Servidor} encerrada", CancellationToken.None);
                }

                // Remove os Clientes do Servidor que será encerrado
                WebSockets.RemoveAll(websocket => websocket.Servidor == Servidor);

                // Fecha o Servidor
                httpListener.Close();

                // Remove o Servidor da Lista
                Servidores.Remove(Servidor);

                // Exibe a mensagem
                WebSocketADO.GERA_LOG(Connection, $"Servidor {Servidor} parado com sucesso.");
                Console.WriteLine($"Servidor {Servidor} parado com sucesso.");

                // Para o timer de validação de sessão
                timerValidadorSessao?.Change(Timeout.Infinite, Timeout.Infinite);
                timerValidadorSessao?.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                WebSocketADO.GERA_LOG(Connection, $"Exceção gerada ao tentar encerrar o servidor WebSocket: {ex.Message}");
                Console.WriteLine($"Exceção gerada ao tentar encerrar o servidor WebSocket: {ex.Message}");
                return false;
            }
        }

        private void ValidaSessaoCliente(object state)
        {
            // Verifica qual websocket ja expirou
            foreach (var socket in WebSockets)
            {
                if (DateTime.Now > socket.DataExpiracao)
                {
                    // Desconecta o Usuário
                    socket.Conector.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão Encerrada pelo Servidor", CancellationToken.None);
                }
            }
        }
    }
}
