using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.Security;
using System.Security.Cryptography.X509Certificates;
using WebsupplyEmar.API.Helpers;
using WebsupplyEmar.API.Services;

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : Controller
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IConfiguration _configuration;
        private readonly WebSocketHelper _webSocketHelper;
        
        // Dados do Websocket
        private static WebSocketInfo _webSocketInfo = new WebSocketInfo();

        public WebSocketController(WebSocketHelper webSocketHelper, IConfiguration configuration, IWebSocketService webSocketService)
        {
            _webSocketHelper = webSocketHelper;
            _configuration = configuration;
            _webSocketService = webSocketService;

            // Define a string de Conexão com o Banco para Gerar os Logs
            _webSocketHelper.DefineConexaoBD(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            // Assinar o evento OnSpecificMessageReceived
            _webSocketHelper.OnMensagemEspecificaRecebida += (message) =>
            {
                Console.WriteLine($"Mensagem Específica Recebida: {message}");

                // Adicione o código adicional que deseja executar quando a mensagem específica for recebida.
            };
        }

        [HttpGet("inicia_servidor")]
        public async Task<IActionResult> IniciaServidor()
        {
            // Verifica se o Servidor não foi inicializado realiza o start, caso sim
            // retorna mensagem de erro
            if (!_webSocketInfo.ServidorOnline)
            {
                // Define o Servidor
                string Prefixo = Request.Scheme == "https" ? "https://" : "http://";
                string Servidor = _configuration.GetValue<string>("WebSockets:Host");
                string Porta = ":" + (Request.Scheme == "https" ? _configuration.GetValue<string>("WebSockets:PortaSSL") : _configuration.GetValue<string>("WebSockets:Porta")) + "/";

                // Pega os Dados do SSL
                X509Certificate2 certSSL = SSLHelper.ConsultaHashCertificado(Servidor);

                // Seta os Dados do Servidor
                _webSocketInfo = new WebSocketInfo
                {
                    Host = Prefixo + Servidor + Porta,
                    DataHorarioInicio = DateTime.Now,
                    ServidorOnline = true,
                    SSL = new WebSocketInfoSSL
                    {
                        Hash = BitConverter.ToString(certSSL.GetCertHash()).Replace("-", ""),
                        InicioValidade = certSSL.NotBefore,
                        FimValidade = certSSL.NotAfter
                    }
                };

                // Seta o Objeto local no serviço 
                _webSocketService.SetWebSocketInfo(_webSocketInfo);

                // Inicia o Servidor
                bool success = await _webSocketHelper.IniciaServidor(Prefixo + Servidor + Porta);

                // Atualiza os Dados do Servidor
                _webSocketInfo.DataHorarioFim = DateTime.Now;
                _webSocketInfo.ServidorOnline = false;

                // Seta o Objeto local no serviço 
                _webSocketService.SetWebSocketInfo(_webSocketInfo);

                if (success)
                {
                    // Retorna a consulta
                    return APIResponseHelper.EstruturaResponse(
                        "Sucesso",
                        "O Servidor foi iniciado e sua execução foi finalizada com sucesso",
                        "success",
                        null,
                        200,
                        Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
                }
                else
                {
                    // Retorna a consulta
                    return APIResponseHelper.EstruturaResponse(
                        "Ops",
                        "Erro ao iniciar o servidor websocket",
                        "error",
                        null,
                        400,
                        Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
                }
            }
            else
            {
                // Retorna a consulta
                return APIResponseHelper.EstruturaResponse(
                    "Ops",
                    "O Servidor já foi inicializado",
                    "error",
                    null,
                    400,
                    Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
            }
        }

        [HttpGet("fecha_servidor")]
        public async Task<IActionResult> FechaServidor()
        {
            // Verifica se o Servidor foi inicializado realiza o encerramento, caso não
            // retorna mensagem de erro
            if (_webSocketInfo.ServidorOnline)
            {
                // Define o Servidor
                string Prefixo = Request.Scheme == "https" ? "https://" : "http://";
                string Servidor = _configuration.GetValue<string>("WebSockets:Host");
                string Porta = ":" + (Request.Scheme == "https" ? _configuration.GetValue<string>("WebSockets:PortaSSL") : _configuration.GetValue<string>("WebSockets:Porta")) + "/";

                // Para o Servidor
                bool success = await _webSocketHelper.FechaServidor(Prefixo + Servidor + Porta);

                // Atualiza os Dados do Servidor
                _webSocketInfo.DataHorarioFim = DateTime.Now;
                _webSocketInfo.ServidorOnline = false;

                // Seta o Objeto local no serviço 
                _webSocketService.SetWebSocketInfo(_webSocketInfo);

                if (success)
                {
                    // Retorna a consulta
                    return APIResponseHelper.EstruturaResponse(
                        "Sucesso",
                        "O Servidor foi finalizado com sucesso",
                        "success",
                        null,
                        200,
                        Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
                }
                else
                {
                    // Retorna a consulta
                    return APIResponseHelper.EstruturaResponse(
                        "Ops",
                        "Erro ao finalizar o servidor websocket",
                        "error",
                        null,
                        400,
                        Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
                }
            }
            else
            {
                // Retorna a consulta
                return APIResponseHelper.EstruturaResponse(
                    "Ops",
                    "O Servidor já foi encerrado",
                    "error",
                    null,
                    400,
                    Url.Action("fecha_servidor", "WebSocket", null, Request.Scheme));
            }
        }

        [HttpGet("info_servidor")]
        public async Task<IActionResult> InfoServidor()
        {
            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Informações do Servidor WebSocket Consultadas com Sucesso",
                "success",
                _webSocketInfo,
                200,
                Url.Action("info_servidor", "WebSocket", null, Request.Scheme));
        }
    }
}
