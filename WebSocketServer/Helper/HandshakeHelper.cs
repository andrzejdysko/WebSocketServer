using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace WebSocketServer.Helper
{
    public static class HandshakeHelper
    {
        public static async Task<bool> PerformHandshakeAsync(NetworkStream stream)
        {
            var reader = new StreamReader(stream, Encoding.UTF8);
            var request = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(request) || !request.Contains("GET"))
                return false;

            string secWebSocketKey = null;

            while (!string.IsNullOrEmpty(request))
            {
                if (request.StartsWith("Sec-WebSocket-Key:"))
                {
                    secWebSocketKey = request.Substring(19).Trim();
                }
                request = await reader.ReadLineAsync();
            }

            if (string.IsNullOrEmpty(secWebSocketKey))
                return false;

            string secWebSocketAccept = ComputeWebSocketAcceptString(secWebSocketKey);

            var response = "HTTP/1.1 101 Switching Protocols\r\n" +
                           "Upgrade: websocket\r\n" +
                           "Connection: Upgrade\r\n" +
                           $"Sec-WebSocket-Accept: {secWebSocketAccept}\r\n\r\n";

            var responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

            return true;
        }

        private static string ComputeWebSocketAcceptString(string secWebSocketKey)
        {
            string keyConcat = secWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(keyConcat));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
