namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var server = new TcpServer(4000);
            server.StartAsync().GetAwaiter().GetResult();
        }
    }
}