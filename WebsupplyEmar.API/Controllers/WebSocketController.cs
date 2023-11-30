using Microsoft.AspNetCore.Mvc;
using WebsupplyEmar.API.Helpers;

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : Controller
    {
        private readonly WebSocketHelper _webSocketHelper;

        public WebSocketController(WebSocketHelper webSocketHelper)
        {
            _webSocketHelper = webSocketHelper;

            // Assinar o evento OnSpecificMessageReceived
            _webSocketHelper.OnMensagemEspecificaRecebida += (message) =>
            {
                Console.WriteLine($"Mensagem Específica Recebida: {message}");

                // Adicione o código adicional que deseja executar quando a mensagem específica for recebida.
            };
        }

        [HttpGet("inicia_servidor")]
        public async Task<IActionResult> IniciaServidor(string Servidor)
        {
            bool success = await _webSocketHelper.IniciaServidor(Servidor);

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
