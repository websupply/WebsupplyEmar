using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebsupplyEmar.Dominio.Model;

namespace WebsupplyEmar.API.Helpers
{
    public class EmarHelper
    {
        public static bool EnviaMensagemWebSocket(EmarLogProcessamentoModel logProcessamentoModel, ClientWebSocket webSocketCliente, bool finalizaWebSocket = false)
        {
            try
            {
                // Define o Buffer com o conteudo da mensagem
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(logProcessamentoModel));

                // Envia a Mensagem para o WebSocket
                webSocketCliente.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, default).Wait();

                // Encerra a Conexão com o WebSocket
                if (finalizaWebSocket)
                {
                    webSocketCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexão finalizada pós-processamento", CancellationToken.None);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
