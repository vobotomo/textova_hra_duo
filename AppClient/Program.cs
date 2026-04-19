using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStation.Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        ConsoleUI.ShowBanner();

        string host = "127.0.0.1";
        int port = 4000;

        if (args.Length >= 1) host = args[0];
        if (args.Length >= 2 && int.TryParse(args[1], out int p)) port = p;

        ConsoleUI.WriteSystem($"Připojuji se k {host}:{port}...");

        try
        {
            using var client = new NexusClient(host, port);
            await client.ConnectAsync();
            await client.RunAsync();
        }
        catch (SocketException ex)
        {
            ConsoleUI.WriteError($"Nelze se připojit k serveru: {ex.Message}");
            ConsoleUI.WriteSystem("Ujistěte se, že server běží a zkuste to znovu.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Neočekávaná chyba: {ex.Message}");
        }

        ConsoleUI.WriteSystem("Stiskněte libovolnou klávesu pro ukončení...");
        Console.ReadKey();
    }
}