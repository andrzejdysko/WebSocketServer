using System.Net.Sockets;
using WebSocketServer.Helper;

namespace WebSocketServer.Server
{
    public class WebSocketClient
    {
        private readonly TcpClient _tcpClient;

        public WebSocketClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public async Task HandleClientAsync()
        {
            var stream = _tcpClient.GetStream();

            if (!await HandshakeHelper.PerformHandshakeAsync(stream))
            {
                Console.WriteLine("Handshake failed.");
                return;
            }

            Console.WriteLine("Handshake successful.");

            while (_tcpClient.Connected)
            {
                var message = await WebSocketFrame.ReadFrameAsync(stream);
                if (message != null)
                {
                    Console.WriteLine($"Received message: {message}");
                    await WebSocketFrame.WriteFrameAsync(stream, $"Reflected {message}" );
                }
            }
        }
    }
}
