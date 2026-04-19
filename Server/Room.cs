using System.Collections.Generic;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, int> Exits { get; set; } = new Dictionary<string, int>();
    public List<string> Items { get; set; } = new List<string>();
    public List<string> Npcs { get; set; } = new List<string>();
}