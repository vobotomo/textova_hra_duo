using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStation.Client;

public class NexusClient : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _running;
    private readonly CancellationTokenSource _cts = new();

    public NexusClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(_host, _port);
        var stream = _tcpClient.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        ConsoleUI.WriteSuccess("Připojeno k Nexus Station!");
    }

    public async Task RunAsync()
    {
        _running = true;
        
        var receiveTask = ReceiveLoopAsync(_cts.Token);
        
        await InputLoopAsync(_cts.Token);

        _cts.Cancel();
        try { await receiveTask; } catch { }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _reader != null)
            {
                var line = await _reader.ReadLineAsync(ct);
                if (line == null) break;

                ConsoleUI.DisplayServerMessage(line);
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Chyba při příjmu: {ex.Message}");
        }

        if (_running)
        {
            ConsoleUI.WriteSystem("Spojení se serverem bylo přerušeno.");
            _running = false;
            _cts.Cancel();
        }
    }

    private async Task InputLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _running)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("> ");
                Console.ResetColor();

                string? input = await Task.Run(() =>
                {
                    try { return Console.ReadLine(); }
                    catch { return null; }
                }, ct);

                if (input == null || ct.IsCancellationRequested) break;

                if (string.IsNullOrWhiteSpace(input)) continue;
                
                if (input.Trim().ToLower() == "exit" || input.Trim().ToLower() == "quit")
                {
                    await SendAsync("odhlasit");
                    break;
                }

                if (input.Trim().ToLower() == "clear" || input.Trim().ToLower() == "cls")
                {
                    Console.Clear();
                    ConsoleUI.ShowBanner();
                    continue;
                }

                await SendAsync(input);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                ConsoleUI.WriteError($"Chyba vstupu: {ex.Message}");
                break;
            }
        }

        _running = false;
    }

    private async Task SendAsync(string message)
    {
        if (_writer == null) return;
        try
        {
            await _writer.WriteLineAsync(message);
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Chyba při odesílání: {ex.Message}");
            _running = false;
            _cts.Cancel();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _reader?.Dispose();
        _writer?.Dispose();
        _tcpClient?.Dispose();
    }
}
