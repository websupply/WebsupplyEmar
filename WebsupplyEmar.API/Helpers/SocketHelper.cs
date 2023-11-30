using System.Net;
using System.Net.Sockets;

namespace WebsupplyEmar.API.Helpers
{
    public class Listener
    {
        Socket socket;

        public bool Listening { get; private set; }
        public int Port { get; private set; }
        
        public Listener(int port)
        {
            Port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (Listening) { return; }

            socket.Bind(new IPEndPoint(0, Port));
            socket.Listen(0);

            socket.BeginAccept(callback, null);
            Listening = true;
        }

        public void Stop()
        {
            if(!Listening) { return; }

            socket.Close();
            socket.Dispose();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        void callback(IAsyncResult asyncResult)
        {
            try
            {
                Socket socket = this.socket.EndAccept(asyncResult);

                if(SocketAccepted != null)
                {
                    SocketAccepted(socket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public delegate void SocketAcceptedHandler(Socket e);
        public event SocketAcceptedHandler SocketAccepted;
    }

    public class SocketHelper
    {
        static Listener listener;
        static List<Socket> sockets;

        static SocketHelper()
        {
            listener = new Listener(8);
            listener.SocketAccepted += new Listener.SocketAcceptedHandler(listener_SocketAccepted);
            listener.Start();

            Console.Read();
        }

        static void listener_SocketAccepted(System.Net.Sockets.Socket socket)
        {
            Console.WriteLine("New Connection: {0}\n{1}\n===========", socket.RemoteEndPoint, DateTime.Now);
            sockets.Add(socket);
        }
    }
}
