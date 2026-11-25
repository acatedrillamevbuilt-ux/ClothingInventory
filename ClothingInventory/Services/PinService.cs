using System.Security.Cryptography;
using ClothingInventory.Data;

namespace ClothingInventory.Services
{
    public enum PinResult { Ok, Wrong, Locked }

    public class PinService
    {
        private const string HashKey = "StaffPinHash";
        private const string FailKey = "StaffPinFailures";
        private const string LockKey = "StaffPinLockedUntil";
        private const int Iterations = 120_000;
        private const int AttemptsPerRound = 5;

        public bool IsPinSet() => !string.IsNullOrEmpty(SettingsStore.Get(HashKey));

        // Returns a short problem description, or null if the PIN is acceptable
        public static string? CheckRules(string pin)
        {
            if (pin.Length < 4 || pin.Length > 12) return "Use 4 to 12 digits";
            if (!pin.All(char.IsAsciiDigit)) return "Digits only";
            if (pin.Distinct().Count() == 1) return "Don't repeat one digit";
            if (IsSequence(pin)) return "Avoid sequences like 1234";
            return null;
        }

        public (bool Success, string Message) SetPin(string pin)
        {
            string? problem = CheckRules(pin);
            if (problem != null) return (false, problem);

            SettingsStore.Set(HashKey, Hash(pin));
            ResetFailures();
            return (true, "PIN saved.");
        }

        // How long sign-in is still locked, or null if it is not locked
        public TimeSpan? LockRemaining()
        {
            if (!long.TryParse(SettingsStore.Get(LockKey), out long ticks)) return null;

            var left = new DateTime(ticks, DateTimeKind.Utc) - DateTime.UtcNow;
            return left > TimeSpan.Zero ? left : null;
        }

        // Checks a PIN and applies the lockout rules
        public PinResult Verify(string pin, out int attemptsLeft, out TimeSpan lockFor)
        {
            attemptsLeft = 0;
            lockFor = TimeSpan.Zero;

            var remaining = LockRemaining();
            if (remaining != null)
            {
                lockFor = remaining.Value;
                return PinResult.Locked;
            }

            string? stored = SettingsStore.Get(HashKey);
            if (!string.IsNullOrEmpty(stored) && Matches(pin, stored))
            {
                ResetFailures();
                return PinResult.Ok;
            }

            int failures = int.TryParse(SettingsStore.Get(FailKey), out int f) ? f + 1 : 1;
            SettingsStore.Set(FailKey, failures.ToString());

            if (failures % AttemptsPerRound == 0)
            {
                int round = failures / AttemptsPerRound;                 // 1, 2, 3 ...
                int minutes = Math.Min(15, 1 << Math.Min(round - 1, 4)); // 1, 2, 4, 8, 15
                lockFor = TimeSpan.FromMinutes(minutes);
                SettingsStore.Set(LockKey, DateTime.UtcNow.Add(lockFor).Ticks.ToString());
                return PinResult.Locked;
            }

            attemptsLeft = AttemptsPerRound - failures % AttemptsPerRound;
            return PinResult.Wrong;
        }

        public static string Describe(TimeSpan t) =>
            t.TotalSeconds < 90
                ? $"{Math.Max(1, (int)Math.Ceiling(t.TotalSeconds))} second(s)"
                : $"{(int)Math.Ceiling(t.TotalMinutes)} minute(s)";

        private static void ResetFailures() => SettingsStore.Delete(FailKey, LockKey);

        private static bool IsSequence(string pin)
        {
            bool up = true, down = true;
            for (int i = 1; i < pin.Length; i++)
            {
                if (pin[i] - pin[i - 1] != 1) up = false;
                if (pin[i - 1] - pin[i] != 1) down = false;
            }
            return up || down;
        }

        private static string Hash(string pin)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, 32);
            return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool Matches(string pin, string stored)
        {
            string[] parts = stored.Split('.');
            if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out int iterations))
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch
            {
                return false;
            }
        }
    }
}