using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace Server
{
    public static class JsonLoader
    {
        public static void LoadWorld(string path, WorldManager world, NpcManager npcManager)
        {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<WorldData>(json);

            foreach (var room in data.Rooms)
                world.AddRoom(room);

            foreach (var npc in data.Npcs)
                npcManager.AddNpc(npc);
        }
    }

    public class WorldData
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Npc> Npcs { get; set; } = new List<Npc>();
    }
}