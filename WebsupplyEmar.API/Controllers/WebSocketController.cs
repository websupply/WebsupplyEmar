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
            // Define o Servidor
            string Prefixo = Request.Scheme == "https" ? "https://" : "http://";
            string Servidor = _configuration.GetValue<string>("WebSockets:Host");
            string Porta = ":" + (Request.Scheme == "https" ? _configuration.GetValue<string>("WebSockets:PortaSSL") : _configuration.GetValue<string>("WebSockets:Porta")) + "/";

            // Inicia o Servidor
            bool success = await _webSocketHelper.IniciaServidor(Prefixo + Servidor + Porta);

            if (success)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Falha ao iniciar o servidor WebSocket");
            }
        }

        [HttpGet("fecha_servidor")]
        public async Task<IActionResult> FechaServidor(string Servidor)
        {
            bool success = await _webSocketHelper.FechaServidor(Servidor);

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
