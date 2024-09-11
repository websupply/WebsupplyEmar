using Microsoft.AspNetCore.Mvc;
using WebsupplyEmar.API.Helpers;

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly WebSocketHelper _webSocketHelper;
        
        // Dados do Websocket
        private static WebSocketInfo _webSocketInfo = new WebSocketInfo();

        public WebSocketController(WebSocketHelper webSocketHelper, IConfiguration configuration)
        {
            _webSocketHelper = webSocketHelper;
            _configuration = configuration;

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

                // Seta os Dados do Servidor
                _webSocketInfo = new WebSocketInfo
                {
                    Host = Prefixo + Servidor + Porta,
                    DataHorarioInicio = DateTime.Now,
                    ServidorOnline = true
                };

                // Inicia o Servidor
                bool success = await _webSocketHelper.IniciaServidor(Prefixo + Servidor + Porta);

                // Atualiza os Dados do Servidor
                _webSocketInfo.DataHorarioFim = DateTime.Now;
                _webSocketInfo.ServidorOnline = false;

                if (success)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Falha ao iniciar o servidor WebSocket");
                }
            }
            else
            {
                return StatusCode(400, "O Servidor já foi inicializado");
            }
        }

        [HttpGet("fecha_servidor")]
        public async Task<IActionResult> FechaServidor()
        {
            // Define o Servidor
            string Prefixo = Request.Scheme == "https" ? "https://" : "http://";
            string Servidor = _configuration.GetValue<string>("WebSockets:Host");
            string Porta = ":" + (Request.Scheme == "https" ? _configuration.GetValue<string>("WebSockets:PortaSSL") : _configuration.GetValue<string>("WebSockets:Porta")) + "/";

            // Para o Servidor
            bool success = await _webSocketHelper.FechaServidor(Prefixo + Servidor + Porta);

            if (success)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Falha ao fechar o servidor WebSocket");
            }
        }
    }
}
