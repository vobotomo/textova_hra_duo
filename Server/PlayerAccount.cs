using System.Collections.Generic;

namespace Server
{
    /// <summary>
    /// Persistentní data hráče uložená na disku jako JSON.
    /// Heslo je uloženo jako SHA-256 hash se solí – nikdy v čistém textu.
    /// </summary>
    public class PlayerAccount
    {
        // --- Přihlašovací údaje ---
        public string Username   { get; set; } = "";
        public string PasswordHash { get; set; } = "";   // SHA-256(salt + password)
        public string Salt       { get; set; } = "";     // náhodná 16B sůl v hex

        // --- Herní stav (obnovuje se při přihlášení) ---
        public int    CurrentRoomId { get; set; } = 1;
        public List<string> Inventory { get; set; } = new List<string>();

        // --- Statistiky / postup ---
        public int    TotalLogins  { get; set; } = 0;
        public string LastLogin    { get; set; } = "";   // ISO 8601
    }
}
