using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class TcpServer
{
    private readonly int _port;
    private TcpListener _listener;
    private bool _running;

    public TcpServer(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _running = true;
        Console.WriteLine($"Server bezi na portu {_port}...");

        while (_running)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Novy hrac se pripojil.");
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            try
            {
                await writer.WriteLineAsync("Vitejte na Nexus Station! Zadejte sve jmeno:");

                string name = await reader.ReadLineAsync();
                Console.WriteLine($"Hrac '{name}' se pripojil.");
                await writer.WriteLineAsync($"Ahoj, {name}! Pis 'pomoc' pro seznam prikazu.");

                while (true)
                {
                    string line = await reader.ReadLineAsync();
                    if (line == null) break;

                    Console.WriteLine($"[{name}]: {line}");
                    await writer.WriteLineAsync($"Prijato: {line}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba klienta: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Hrac se odpojil.");
            }
        }
    }
}