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
        private TcpListener _listener;
        private bool _running;
        private static List<Player> _activePlayers = new List<Player>();
        private static readonly object _playersLock = new object();

        public TcpServer(int port, WorldManager world, NpcManager npcManager)
        {
            _port = port;
            _world = world;
            _npcManager = npcManager;
            _processor = new CommandProcessor(world, npcManager, _activePlayers);
            _listener = new TcpListener(IPAddress.Any, port);
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
                    await writer.WriteLineAsync("Vitejte na Nexus Station! Zadejte sve jmeno:");

                    string name = await reader.ReadLineAsync();
                    player = new Player { Name = name ?? "Neznamy" };

                    lock (_playersLock)
                        _activePlayers.Add(player);

                    Logger.Log("Hrac '" + player.Name + "' se prihlasil z " + endpoint);
                    await writer.WriteLineAsync("Ahoj, " + player.Name + "! Pis 'pomoc' pro seznam prikazu.");

                    while (true)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;

                        Logger.Log("[" + player.Name + "]: " + line);
                        var response = _processor.Process(line, player);
                        foreach (var radek in response.Split('\n'))
                            await writer.WriteLineAsync(radek);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("CHYBA [" + endpoint + "]: " + ex.Message);
                }
                finally
                {
                    if (player != null)
                    {
                        lock (_playersLock)
                            _activePlayers.Remove(player);
                    }
                    Logger.Log("Hrac se odpojil: " + endpoint);
                }
            }
        }
    }
}