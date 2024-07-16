using WSServer = WebSocketServer.Server.WebSocketServer;

internal class Program
{
    private static void Main(string[] args)
    {
        var server = new WSServer("127.0.0.1", 8181);
        server.Start();
    }
}