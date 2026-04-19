namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var world = new WorldManager();

            world.AddRoom(new Room
            {
                Id = 1,
                Name = "Zachranna kapsle",
                Description = "Rozbite svetlo, kovovy zapach. Tady jsi se probudil.",
                Exits = new System.Collections.Generic.Dictionary<string, int> { { "sever", 2 } },
                Items = new System.Collections.Generic.List<string> { "zapisnik" }
            });

            world.AddRoom(new Room
            {
                Id = 2,
                Name = "Chodba A",
                Description = "Polotemna chodba s kabelazi.",
                Exits = new System.Collections.Generic.Dictionary<string, int> { { "jih", 1 }, { "sever", 3 } }
            });

            world.AddRoom(new Room
            {
                Id = 3,
                Name = "Reaktorova hala",
                Description = "Obrovska mistnost humici chladicimi ventilatory.",
                Exits = new System.Collections.Generic.Dictionary<string, int> { { "jih", 2 } },
                Items = new System.Collections.Generic.List<string> { "klic od skladu" },
                Npcs = new System.Collections.Generic.List<string> { "Technik Orion" }
            });

            var server = new TcpServer(4000, world);
            server.StartAsync().GetAwaiter().GetResult();
        }
    }
}