using System.Collections.Generic;

namespace Server
{
    public class NpcManager
    {
        private Dictionary<string, Npc> _npcs = new Dictionary<string, Npc>();

        public void AddNpc(Npc npc)
        {
            _npcs[npc.Name.ToLower()] = npc;
        }

        public Npc GetNpc(string name)
        {
            _npcs.TryGetValue(name.ToLower(), out var npc);
            return npc;
        }
    }
}