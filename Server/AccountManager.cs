using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Server
{
    /// <summary>
    /// Správce uživatelských účtů.
    ///
    /// Každý účet je uložen jako samostatný JSON soubor ve složce <c>accounts/</c>
    /// vedle spustitelného souboru serveru.  Hesla jsou hashována algoritmem
    /// SHA-256 s náhodnou 16bajtovou solí (hex) – v čistém textu se nikde
    /// neukládají ani nelogují.
    /// </summary>
    public class AccountManager
    {
        private readonly string _accountsDir;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public AccountManager(string accountsDir = "accounts")
        {
            _accountsDir = accountsDir;
            Directory.CreateDirectory(_accountsDir);
        }

        // ── Veřejné API ──────────────────────────────────────────────────────

        /// <summary>Vrátí true a vyplní <paramref name="account"/> při úspěšném přihlášení.</summary>
        public bool TryLogin(string username, string password, out PlayerAccount account)
        {
            account = null;
            var path = AccountPath(username);
            if (!File.Exists(path)) return false;

            var loaded = Load(path);
            if (loaded == null) return false;

            if (!VerifyPassword(password, loaded.Salt, loaded.PasswordHash))
                return false;

            loaded.TotalLogins++;
            loaded.LastLogin = DateTime.UtcNow.ToString("o");
            Save(loaded);

            account = loaded;
            return true;
        }

        /// <summary>
        /// Registruje nový účet.  Vrátí false, pokud již existuje.
        /// </summary>
        public bool TryRegister(string username, string password, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(username) || username.Length < 2)
            { error = "Jméno musí mít alespoň 2 znaky."; return false; }

            if (username.Length > 24)
            { error = "Jméno může mít nejvýše 24 znaků."; return false; }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            { error = "Heslo musí mít alespoň 4 znaky."; return false; }

            if (File.Exists(AccountPath(username)))
            { error = "Hráč s tímto jménem již existuje."; return false; }

            var (hash, salt) = HashPassword(password);
            var account = new PlayerAccount
            {
                Username     = username,
                PasswordHash = hash,
                Salt         = salt,
                CurrentRoomId = 1,
                TotalLogins  = 1,
                LastLogin    = DateTime.UtcNow.ToString("o")
            };

            Save(account);
            return true;
        }

        /// <summary>Uloží aktuální herní stav hráče (volá se při odpojení).</summary>
        public void SavePlayerState(Player player)
        {
            var path = AccountPath(player.Name);
            if (!File.Exists(path)) return;          // účet musí existovat

            var account = Load(path);
            if (account == null) return;

            account.CurrentRoomId = player.CurrentRoomId;
            account.Inventory     = new System.Collections.Generic.List<string>(player.Inventory);

            Save(account);
            Logger.Log($"Stav hráče '{player.Name}' uložen (místnost {player.CurrentRoomId}, inventář: {player.Inventory.Count} předmětů).");
        }

        // ── Pomocné metody ───────────────────────────────────────────────────

        private string AccountPath(string username)
            => Path.Combine(_accountsDir, SanitizeFilename(username) + ".json");

        private static string SanitizeFilename(string name)
        {
            // Ponecháme jen bezpečné znaky, aby uživatel nemohl cestovat po filesystému.
            var sb = new StringBuilder();
            foreach (char c in name)
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                    sb.Append(char.ToLower(c));
            return sb.Length > 0 ? sb.ToString() : "unknown";
        }

        private PlayerAccount Load(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<PlayerAccount>(json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Chyba při načítání účtu '{path}': {ex.Message}");
                return null;
            }
        }

        private void Save(PlayerAccount account)
        {
            try
            {
                string json = JsonSerializer.Serialize(account, _jsonOpts);
                File.WriteAllText(AccountPath(account.Username), json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Chyba při ukládání účtu '{account.Username}': {ex.Message}");
            }
        }

        // ── Hashování hesel ──────────────────────────────────────────────────

        private static (string hash, string salt) HashPassword(string password)
        {
            // Generujeme 16 náhodných bajtů jako sůl.
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            string salt = Convert.ToHexString(saltBytes);           // 32 znaků hex
            string hash = ComputeHash(salt, password);
            return (hash, salt);
        }

        private static bool VerifyPassword(string password, string salt, string expectedHash)
            => ComputeHash(salt, password) == expectedHash;

        private static string ComputeHash(string salt, string password)
        {
            // SHA-256(salt + password) – sůl je vždy před heslem
            byte[] input = Encoding.UTF8.GetBytes(salt + password);
            byte[] hashBytes = SHA256.HashData(input);
            return Convert.ToHexString(hashBytes);                  // 64 znaků hex
        }
    }
}
