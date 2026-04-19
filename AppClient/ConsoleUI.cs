using System;

namespace NexusStation.Client;

public static class ConsoleUI
{
    private static readonly object _lock = new();

    public static void ShowBanner()
    {
        lock (_lock)
        {
            Console.Clear();
            Console.WriteLine("  ─────────────────────────────────────────────────────────────");
            Console.WriteLine("   Textové sci-fi MUD | Zadej 'pomoc' pro seznam příkazů");
            Console.WriteLine("   'clear' = vyčisti obrazovku | 'exit' = odhlás se a skonči");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
    public static void DisplayServerMessage(string line)
    {
        lock (_lock)
        {
            int currentLeft = Console.CursorLeft;
            if (currentLeft > 0)
            {
                Console.Write("\r" + new string(' ', currentLeft + 2) + "\r");
            }

            if (line.StartsWith(">>>") && line.EndsWith("<<<"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  " + line);
            }
            else if (line.StartsWith("Vychody:") || line.StartsWith("Východy:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("  Východy: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(line.Substring(line.IndexOf(':') + 1));
            }
            else if (line.StartsWith("Predmety:") || line.StartsWith("Předměty:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("  Předměty: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(line.Substring(line.IndexOf(':') + 1));
            }
            else if (line.StartsWith("NPC:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("  NPC: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(line.Substring(4));
            }
            else if (line.StartsWith("Hraci:") || line.StartsWith("Hráči:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("  Hráči: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(line.Substring(line.IndexOf(':') + 1));
            }
            else if (line.StartsWith("Inventar:") || line.StartsWith("Inventář:") || line.Contains("Kapacita:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("  " + line);
            }
            else if (line.StartsWith("Nelze") || line.StartsWith("Chyba") || line.StartsWith("CHYBA")
                     || line.Contains("neexistuje") || line.Contains("není") && line.Contains("!"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  " + line);
            }
            else if (line.StartsWith("Prihlaseni") || line.StartsWith("Přihlášení")
                     || line.StartsWith("Vitejte") || line.StartsWith("Vítejte")
                     || line.StartsWith("Uspesne") || line.StartsWith("Úspěšně"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  " + line);
            }
            else if (line.Contains("GRATULACE") || line.Contains("dokoncil") || line.Contains("dokončil")
                     || line.StartsWith("=== Zebricek") || line.StartsWith("=== Žebříček"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(line);
            }
            else if (line.StartsWith("#") || (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.'))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  " + line);
            }
            else if (line.StartsWith("[") && line.Contains("]"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(line);
            }
            else if (line.StartsWith("Zadejte") || line.StartsWith("Zadej")
                     || line.StartsWith("SERVER:") || line.StartsWith(">>"))
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(line);
            }
            else if (line.StartsWith("---") || line.StartsWith("==="))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(line);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(line);
            }

            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("> ");
            Console.ResetColor();
        }
    }

    public static void WriteError(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[CHYBA] {message}");
            Console.ResetColor();
        }
    }

    public static void WriteSuccess(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {message}");
            Console.ResetColor();
        }
    }

    public static void WriteSystem(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[SYSTÉM] {message}");
            Console.ResetColor();
        }
    }
}
