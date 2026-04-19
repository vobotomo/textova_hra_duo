using System.Collections.Generic;

namespace Server
{
    public class CommandProcessor
    {
        private readonly WorldManager _world;

        private readonly NpcManager _npcManager;

        private readonly List<Player> _activePlayers;

        public CommandProcessor(WorldManager world, NpcManager npcManager, List<Player> activePlayers)
        {
            _world = world;
            _npcManager = npcManager;
            _activePlayers = activePlayers;
        }
        public string Process(string input, Player player)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Nezadal jsi zadny prikaz.";

            var parts = input.Trim().ToLower().Split(' ', 2);
            var command = parts[0].ToLower();
            var argument = parts.Length > 1 ? parts[1] : "";

            return command switch
            {
                "prozkoumej" => Prozkoumej(player),
                "jdi"        => Jdi(player, argument),
                "vezmi"      => Vezmi(player, argument),
                "poloz"      => Poloz(player, argument),
                "inventar"   => Inventar(player),
                "pomoc"      => Pomoc(),
                "mluv" => Mluv(player, argument),
                _            => "Neznam tento prikaz. Pis 'pomoc' pro seznam prikazu."
            };
        }
        
        private string Mluv(Player player, string jmeno)
        {
            if (string.IsNullOrWhiteSpace(jmeno))
                return "S kym chces mluvit? Napr: mluv technik orion";

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room == null) return "Mistnost nenalezena.";

            string found = room.Npcs.Find(n => n.ToLower() == jmeno.ToLower());
            if (found == null)
                return "'" + jmeno + "' tady neni.";

            var npc = _npcManager.GetNpc(jmeno);
            if (npc == null) return "'" + jmeno + "' nema co rict.";

            return npc.Name + ": \"" + npc.Dialog + "\"";
        }

        private string Prozkoumej(Player player)
        {
            var room = _world.GetRoom(player.CurrentRoomId);
            return room == null ? "Mistnost nenalezena." : _world.DescribeRoom(room, _activePlayers, player);
        }

        private string Jdi(Player player, string smer)
        {
            if (string.IsNullOrWhiteSpace(smer))
                return "Zadej smer. Napr: jdi sever";

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room == null) return "Mistnost nenalezena.";

            if (room.Exits.TryGetValue(smer.ToLower(), out int nextRoomId))
            {
                player.CurrentRoomId = nextRoomId;
                var nextRoom = _world.GetRoom(nextRoomId);
                return nextRoom == null ? "Mistnost nenalezena." : _world.DescribeRoom(nextRoom, _activePlayers, player);
            }

            return "Smer '" + smer + "' tady neexistuje.";
        }

        private string Vezmi(Player player, string predmet)
        {
            if (string.IsNullOrWhiteSpace(predmet))
                return "Co chces vzit? Napr: vezmi zapisnik";

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room == null) return "Mistnost nenalezena.";

            string found = room.Items.Find(i => i.ToLower() == predmet.ToLower());
            if (found == null)
                return "Predmet '" + predmet + "' tady neni.";

            if (player.Inventory.Count >= player.MaxCapacity)
                return "Inventar je plny! Maximalni kapacita: " + player.MaxCapacity;

            room.Items.Remove(found);
            player.Inventory.Add(found);
            return "Vzal jsi: " + found;
        }

        private string Poloz(Player player, string predmet)
        {
            if (string.IsNullOrWhiteSpace(predmet))
                return "Co chces polozit? Napr: poloz zapisnik";

            string found = player.Inventory.Find(i => i.ToLower() == predmet.ToLower());
            if (found == null)
                return "'" + predmet + "' nemas v inventari.";

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room == null) return "Mistnost nenalezena.";

            player.Inventory.Remove(found);
            room.Items.Add(found);
            return "Polozil jsi: " + found;
        }

        private string Inventar(Player player)
        {
            if (player.Inventory.Count == 0)
                return "Inventar je prazdny. (kapacita: " + player.MaxCapacity + ")";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Inventar (" + player.Inventory.Count + "/" + player.MaxCapacity + "):");
            foreach (var item in player.Inventory)
                sb.AppendLine("  - " + item);
            return sb.ToString();
        }

        private string Pomoc()
        {
            return
                "\nSEZNAM PRIKAZU:\n\n" +
                "  prozkoumej        - zobrazi aktualni mistnost\n" +
                "  jdi <smer>        - pohyb, napr: jdi sever\n" +
                "  vezmi <predmet>   - vezmi predmet z mistnosti\n" +
                "  poloz <predmet>   - poloz predmet z inventare\n" +
                "  inventar          - zobrazi inventar\n" +
                "  pomoc             - zobrazi tento seznam\n" +
                "  mluv <jmeno>     - promluv s NPC, napr: mluv Technik Orion\n";
        }
    }
}