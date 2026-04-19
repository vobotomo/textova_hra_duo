using System.Collections.Generic;

namespace Server
{
    public class WorldManager
    {
        private Dictionary<int, Room> _rooms = new Dictionary<int, Room>();

        public void AddRoom(Room room)
        {
            _rooms[room.Id] = room;
        }

        public Room GetRoom(int id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        public string DescribeRoom(Room room)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\n=== " + room.Name + " ===");
            sb.AppendLine(room.Description);
            sb.AppendLine("Vychody: " + (room.Exits.Count > 0 ? string.Join(", ", room.Exits.Keys) : "zadne"));
            sb.AppendLine("Predmety: " + (room.Items.Count > 0 ? string.Join(", ", room.Items) : "zadne"));
            sb.AppendLine("NPC: " + (room.Npcs.Count > 0 ? string.Join(", ", room.Npcs) : "nikdo"));
            return sb.ToString();
        }
    }
}