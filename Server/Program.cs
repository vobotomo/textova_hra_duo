namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var world = new WorldManager();
            var npcManager = new NpcManager();

            JsonLoader.LoadWorld("world.json", world, npcManager);

            var server = new TcpServer(4000, world, npcManager);
            server.StartAsync().GetAwaiter().GetResult();
        }
    }
}