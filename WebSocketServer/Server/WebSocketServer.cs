using System.Net.Sockets;
using System.Net;

namespace WebSocketServer.Server
{
    public class WebSocketServer
    {
        private readonly IPAddress _address;
        private readonly int _port;
        private TcpListener _listener;

        public WebSocketServer(string ip, int port)
        {
            _address = IPAddress.Parse(ip);
            _port = port;
        }

        public void Start()
        {
            _listener = new TcpListener(_address, _port);
            _listener.Start();
            Console.WriteLine($"Server started on port {_port}.");

            ListenForClients();
            Console.WriteLine("Press any key to stop the server.");
            Console.ReadLine();
        }

        private async void ListenForClients()
        {
            while (true)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                var client = new WebSocketClient(tcpClient);
                _ = Task.Run(() => client.HandleClientAsync());
            }
        }
    }
}

