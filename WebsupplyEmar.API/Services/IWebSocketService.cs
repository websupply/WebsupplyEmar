using WebsupplyEmar.API.Helpers;

namespace WebsupplyEmar.API.Services
{
    public interface IWebSocketService
    {
        WebSocketInfo GetWebSocketInfo();
        void SetWebSocketInfo(WebSocketInfo info);
    }

    public class WebSocketService : IWebSocketService
    {
        private WebSocketInfo _webSocketInfo = new WebSocketInfo();

        public WebSocketInfo GetWebSocketInfo()
        {
            return _webSocketInfo;
        }

        public void SetWebSocketInfo(WebSocketInfo info)
        {
            _webSocketInfo = info;
        }
    }
}
