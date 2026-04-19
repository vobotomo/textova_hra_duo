using System;
using System.Collections.Generic;

namespace Server
{
    public class CommandProcessor
    {
        private readonly WorldManager _world;

        public CommandProcessor(WorldManager world)
        {
            _world = world;
        }

        public string Process(string input, Player player)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Nezadal jsi zadny prikaz.";

            var parts = input.Trim().Split(' ', 2);
            var command = parts[0].ToLower();
            var argument = parts.Length > 1 ? parts[1] : "";

            return command switch
            {
                "prozkoumej" => Prozkoumej(player),
                "jdi"        => Jdi(player, argument),
                "pomoc"      => Pomoc(),
                _            => "Neznam tento prikaz. Pis 'pomoc' pro seznam prikazu."
            };
        }

        private string Prozkoumej(Player player)
        {
            var room = _world.GetRoom(player.CurrentRoomId);
            return room == null ? "Mistnost nenalezena." : _world.DescribeRoom(room);
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
                return nextRoom == null ? "Mistnost nenalezena." : _world.DescribeRoom(nextRoom);
            }

            return "Smer '" + smer + "' tady neexistuje.";
        }

        private string Pomoc()
        {
            return
                "\nSEZNAM PRIKAZU:\n\n" +
                "  prozkoumej    - zobrazi aktualni mistnost\n" +
                "  jdi <smer>    - pohyb, napr: jdi sever\n" +
                "  pomoc         - zobrazi tento seznam\n";
        }
    }
}