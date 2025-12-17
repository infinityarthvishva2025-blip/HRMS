using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HRMS.Helpers
{
    public static class UrlEncryptionHelper
    {
        private static readonly string Key = "A1B2C3D4E5F6G7H8"; // 16 chars
        private static readonly string IV = "1H2G3F4E5D6C7B8A"; // 16 chars

        // =====================================================
        // 🔐 LEGACY METHOD (Attendance) — DO NOT BREAK
        // =====================================================
        public static string GenerateToken(string empCode, DateTime date, int expiryMinutes = 30)
        {
            return GenerateToken(
                new Dictionary<string, string>
                {
                    ["type"] = "ATT",
                    ["empCode"] = empCode,
                    ["date"] = date.ToString("yyyy-MM-dd")
                },
                expiryMinutes
            );
        }

        public static bool TryDecryptToken(
            string token,
            out string empCode,
            out DateTime date,
            out string error)
        {
            empCode = "";
            date = DateTime.MinValue;
            error = "";

            if (!TryDecryptToken(token, out var fields, out error))
                return false;

            if (!fields.TryGetValue("type", out var type) || type != "ATT")
            {
                error = "Invalid token type";
                return false;
            }

            empCode = fields["empCode"];
            date = DateTime.ParseExact(fields["date"], "yyyy-MM-dd", null);
            return true;
        }

        // =====================================================
        // ✅ UNIVERSAL TOKEN GENERATOR (ANY MODULE)
        // =====================================================
        public static string GenerateToken(
            Dictionary<string, string> fields,
            int expiryMinutes = 30)
        {
            if (fields == null || fields.Count == 0)
                throw new ArgumentException("Token fields cannot be empty");

            var expiryTicks = DateTime.UtcNow.AddMinutes(expiryMinutes).Ticks;

            var payload = string.Join("|",
                fields.Select(kvp => $"{kvp.Key}={kvp.Value}")
            );

            payload += $"|exp={expiryTicks}";

            return Encrypt(payload);
        }

        // =====================================================
        // 🔓 UNIVERSAL TOKEN DECRYPT
        // =====================================================
        public static bool TryDecryptToken(
            string token,
            out Dictionary<string, string> fields,
            out string error)
        {
            fields = new Dictionary<string, string>();
            error = "";

            try
            {
                var decrypted = Decrypt(token);
                var parts = decrypted.Split('|');

                long expiryTicks = 0;

                foreach (var part in parts)
                {
                    var kv = part.Split('=', 2);
                    if (kv.Length != 2) continue;

                    if (kv[0] == "exp")
                        expiryTicks = long.Parse(kv[1]);
                    else
                        fields[kv[0]] = kv[1];
                }

                if (expiryTicks == 0 || DateTime.UtcNow.Ticks > expiryTicks)
                {
                    error = "Token expired";
                    return false;
                }

                return true;
            }
            catch
            {
                error = "Invalid or tampered token";
                return false;
            }
        }

        // =====================================================
        // 🔐 AES ENCRYPTION
        // =====================================================
        private static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace("-", "+").Replace("_", "/");

            switch (cipherText.Length % 4)
            {
                case 2: cipherText += "=="; break;
                case 3: cipherText += "="; break;
            }

            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}


//using System;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;

//namespace HRMS.Helpers
//{
//    public static class UrlEncryptionHelper
//    {
//        private static readonly string Key = "A1B2C3D4E5F6G7H8"; // 16 chars
//        private static readonly string IV = "1H2G3F4E5D6C7B8A"; // 16 chars

//        // 🔐 CREATE TOKEN WITH EXPIRY (minutes)
//        public static string GenerateToken(string empCode, DateTime date, int expiryMinutes = 30)
//        {
//            var expiryUtc = DateTime.UtcNow.AddMinutes(expiryMinutes).Ticks;

//            string payload = $"{empCode}|{date:yyyy-MM-dd}|{expiryUtc}";
//            return Encrypt(payload);
//        }

//        // 🔓 VALIDATE + DECRYPT TOKEN
//        public static bool TryDecryptToken(
//            string token,
//            out string empCode,
//            out DateTime date,
//            out string error)
//        {
//            empCode = "";
//            date = DateTime.MinValue;
//            error = "";

//            try
//            {
//                var decrypted = Decrypt(token);
//                var parts = decrypted.Split('|');

//                if (parts.Length != 3)
//                {
//                    error = "Invalid token format";
//                    return false;
//                }

//                empCode = parts[0];
//                date = DateTime.ParseExact(parts[1], "yyyy-MM-dd", null);

//                long expiryTicks = long.Parse(parts[2]);
//                var expiryUtc = new DateTime(expiryTicks, DateTimeKind.Utc);

//                if (DateTime.UtcNow > expiryUtc)
//                {
//                    error = "Token expired";
//                    return false;
//                }

//                return true;
//            }
//            catch
//            {
//                error = "Invalid or tampered token";
//                return false;
//            }
//        }

//        // ===============================
//        // INTERNAL AES METHODS
//        // ===============================
//        private static string Encrypt(string plainText)
//        {
//            using var aes = Aes.Create();
//            aes.Key = Encoding.UTF8.GetBytes(Key);
//            aes.IV = Encoding.UTF8.GetBytes(IV);

//            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
//            using var ms = new MemoryStream();
//            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
//            using (var sw = new StreamWriter(cs))
//            {
//                sw.Write(plainText);
//            }

//            return Convert.ToBase64String(ms.ToArray())
//                .Replace("+", "-")
//                .Replace("/", "_")
//                .Replace("=", "");
//        }

//        private static string Decrypt(string cipherText)
//        {
//            cipherText = cipherText
//                .Replace("-", "+")
//                .Replace("_", "/");

//            switch (cipherText.Length % 4)
//            {
//                case 2: cipherText += "=="; break;
//                case 3: cipherText += "="; break;
//            }

//            var buffer = Convert.FromBase64String(cipherText);

//            using var aes = Aes.Create();
//            aes.Key = Encoding.UTF8.GetBytes(Key);
//            aes.IV = Encoding.UTF8.GetBytes(IV);

//            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
//            using var ms = new MemoryStream(buffer);
//            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
//            using var sr = new StreamReader(cs);

//            return sr.ReadToEnd();
//        }
//    }
//}
