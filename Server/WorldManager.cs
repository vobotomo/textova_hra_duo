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

        public string DescribeRoom(Room room, List<Player> activePlayers, Player currentPlayer)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\n=== " + room.Name + " ===");
            sb.AppendLine(room.Description);
            sb.AppendLine("Vychody: " + (room.Exits.Count > 0 ? string.Join(", ", room.Exits.Keys) : "zadne"));
            sb.AppendLine("Predmety: " + (room.Items.Count > 0 ? string.Join(", ", room.Items) : "zadne"));
            sb.AppendLine("NPC: " + (room.Npcs.Count > 0 ? string.Join(", ", room.Npcs) : "nikdo"));

            var hraciVMistnosti = activePlayers
                .FindAll(p => p.CurrentRoomId == room.Id && p.Name != currentPlayer.Name);

            sb.AppendLine("Hraci: " + (hraciVMistnosti.Count > 0
                ? string.Join(", ", hraciVMistnosti.ConvertAll(p => p.Name))
                : "nikdo"));

            return sb.ToString();
        }
    }
}