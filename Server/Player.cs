using System.Collections.Generic;

namespace Server
{
    public class Player
    {
        public string Name { get; set; }
        public int CurrentRoomId { get; set; } = 1;
        public List<string> Inventory { get; set; } = new List<string>();
        public int MaxCapacity { get; set; } = 5;
    }
}