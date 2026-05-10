using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    public class TcpServer
    {
        private readonly int _port;
        private readonly WorldManager _world;
        private readonly NpcManager _npcManager;
        private readonly CommandProcessor _processor;
        private readonly AccountManager _accounts;
        private TcpListener _listener;
        private bool _running;
        private static readonly List<Player> _activePlayers = new List<Player>();
        private static readonly object _playersLock = new object();

        public TcpServer(int port, WorldManager world, NpcManager npcManager, AccountManager accounts)
        {
            _port       = port;
            _world      = world;
            _npcManager = npcManager;
            _accounts   = accounts;
            _processor  = new CommandProcessor(world, npcManager, _activePlayers);
            _listener   = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _running = true;
            Logger.Log("Server spusten na portu " + _port);

            while (_running)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Logger.Log("Nove pripojeni: " + client.Client.RemoteEndPoint);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        // ── Obsluha jednoho klienta ──────────────────────────────────────────

        private async Task HandleClientAsync(TcpClient client)
        {
            string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "neznamy";
            Player player = null;

            using (client)
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream) { AutoFlush = true };

                try
                {
                    // ── 1. Přihlašovací smyčka ───────────────────────────────
                    player = await LoginLoopAsync(reader, writer, endpoint);
                    if (player == null) return;

                    // ── 2. Herní smyčka ──────────────────────────────────────
                    await GameLoopAsync(reader, writer, player);
                }
                catch (Exception ex)
                {
                    Logger.Log("CHYBA [" + endpoint + "]: " + ex.Message);
                }
                finally
                {
                    // ── 3. Uložení stavu při odpojení ────────────────────────
                    if (player != null)
                    {
                        _accounts.SavePlayerState(player);
                        lock (_playersLock)
                            _activePlayers.Remove(player);
                        Logger.Log("Hrac '" + player.Name + "' ulozen a odpojen.");
                    }
                    Logger.Log("Spojeni ukonceno: " + endpoint);
                }
            }
        }

        // ── Přihlašovací / registrační smyčka ───────────────────────────────

        private async Task<Player> LoginLoopAsync(StreamReader reader, StreamWriter writer, string endpoint)
        {
            await SendBannerAsync(writer);

            const int maxAttempts = 5;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                await writer.WriteLineAsync("Zadej [1] prihlasit  [2] registrovat  [q] ukoncit:");
                string choice = (await reader.ReadLineAsync())?.Trim().ToLower();

                if (choice == null || choice == "q" || choice == "quit")
                    return null;

                if (choice == "1")
                {
                    var player = await DoLoginAsync(reader, writer);
                    if (player != null) return player;
                    attempts++;
                }
                else if (choice == "2")
                {
                    var player = await DoRegisterAsync(reader, writer);
                    if (player != null) return player;
                    attempts++;
                }
                else
                {
                    await writer.WriteLineAsync("Neznama volba. Zadej 1, 2 nebo q.");
                }
            }

            await writer.WriteLineAsync("Prilis mnoho neuspesnych pokusu. Spojeni ukonceno.");
            return null;
        }

        private async Task<Player> DoLoginAsync(StreamReader reader, StreamWriter writer)
        {
            await writer.WriteLineAsync("--- PRIHLASENI ---");
            await writer.WriteLineAsync("Jmeno:");
            string username = (await reader.ReadLineAsync())?.Trim();

            await writer.WriteLineAsync("Heslo:");
            string password = (await reader.ReadLineAsync())?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await writer.WriteLineAsync("Jmeno ani heslo nesmi byt prazdne.");
                return null;
            }

            if (!_accounts.TryLogin(username, password, out var account))
            {
                await writer.WriteLineAsync("Chybne jmeno nebo heslo.");
                Logger.Log("Neuspesne prihlaseni pro '" + username + "'.");
                return null;
            }

            var player = RestorePlayer(account);
            lock (_playersLock) _activePlayers.Add(player);

            Logger.Log("Hrac '" + player.Name + "' se prihlasil (mistnost " + player.CurrentRoomId + ").");

            await writer.WriteLineAsync("Vitej zpet, " + player.Name + "! (Celkem prihlaseni: " + account.TotalLogins + ")");
            await writer.WriteLineAsync("Posledni prihlaseni: " + account.LastLogin);
            await writer.WriteLineAsync("Pis 'pomoc' pro seznam prikazu.");

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room != null)
            {
                foreach (var line in _world.DescribeRoom(room, _activePlayers, player).Split('\n'))
                    await writer.WriteLineAsync(line);
            }

            return player;
        }

        private async Task<Player> DoRegisterAsync(StreamReader reader, StreamWriter writer)
        {
            await writer.WriteLineAsync("--- REGISTRACE ---");
            await writer.WriteLineAsync("Zvol jmeno (2-24 znaku, pismena/cislice/_/-):");
            string username = (await reader.ReadLineAsync())?.Trim();

            await writer.WriteLineAsync("Zvol heslo (min. 4 znaky):");
            string password = (await reader.ReadLineAsync())?.Trim();

            await writer.WriteLineAsync("Heslo znovu:");
            string password2 = (await reader.ReadLineAsync())?.Trim();

            if (password != password2)
            {
                await writer.WriteLineAsync("Hesla se neshoduji.");
                return null;
            }

            if (!_accounts.TryRegister(username, password, out string error))
            {
                await writer.WriteLineAsync("Registrace selhala: " + error);
                return null;
            }

            Logger.Log("Novy hrac zaregistrovan: '" + username + "'.");

            _accounts.TryLogin(username, password, out var account);
            var player = RestorePlayer(account);
            lock (_playersLock) _activePlayers.Add(player);

            await writer.WriteLineAsync("Ucet vytvoren! Vitej na Nexus Station, " + player.Name + "!");
            await writer.WriteLineAsync("Pis 'pomoc' pro seznam prikazu.");

            var room = _world.GetRoom(player.CurrentRoomId);
            if (room != null)
            {
                foreach (var line in _world.DescribeRoom(room, _activePlayers, player).Split('\n'))
                    await writer.WriteLineAsync(line);
            }

            return player;
        }

        // ── Herní smyčka ────────────────────────────────────────────────────

        private async Task GameLoopAsync(StreamReader reader, StreamWriter writer, Player player)
        {
            while (true)
            {
                string line = await reader.ReadLineAsync();
                if (line == null) break;

                Logger.Log("[" + player.Name + "]: " + line);

                string trimmed = line.Trim().ToLower();

                if (trimmed == "uloz")
                {
                    _accounts.SavePlayerState(player);
                    await writer.WriteLineAsync("Stav hry ulozen.");
                    continue;
                }

                if (trimmed == "odhlasit" || trimmed == "logout" || trimmed == "exit" || trimmed == "quit")
                {
                    await writer.WriteLineAsync("Na shledanou, " + player.Name + "! Stav hry byl ulozen.");
                    break;
                }

                string response = _processor.Process(line, player);
                foreach (var radek in response.Split('\n'))
                    await writer.WriteLineAsync(radek);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Player RestorePlayer(PlayerAccount account)
        {
            return new Player
            {
                Name          = account.Username,
                CurrentRoomId = account.CurrentRoomId,
                Inventory     = new List<string>(account.Inventory),
                MaxCapacity   = 5
            };
        }

        private static async Task SendBannerAsync(StreamWriter writer)
        {
            await writer.WriteLineAsync("=================================================");
            await writer.WriteLineAsync("   NEXUS STATION - Textova sci-fi adventura");
            await writer.WriteLineAsync("=================================================");
        }
    }
}
