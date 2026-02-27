using System;
using System.IO;
using System.Security.Cryptography;

namespace FinX.Api.Services
{
    public class KeyService : IKeyService
    {
        private readonly RSA _private;
        private readonly RSA _public;
        private readonly string _keyId;

        public KeyService()
        {
            var keyDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "keys");
            Directory.CreateDirectory(keyDir);
            var privPath = Path.Combine(keyDir, "private.pem");
            var pubPath = Path.Combine(keyDir, "public.pem");

            if (!File.Exists(privPath) || !File.Exists(pubPath))
            {
                using var rsa = RSA.Create(3072);
                var privBytes = rsa.ExportRSAPrivateKey();
                var pubBytes = rsa.ExportRSAPublicKey();
                File.WriteAllText(privPath, PemEncode("RSA PRIVATE KEY", privBytes));
                File.WriteAllText(pubPath, PemEncode("RSA PUBLIC KEY", pubBytes));
            }

            _private = RSA.Create();
            _private.ImportFromPem(File.ReadAllText(privPath));

            _public = RSA.Create();
            _public.ImportFromPem(File.ReadAllText(pubPath));

            _keyId = Guid.NewGuid().ToString("N");
        }

        public RSA GetPrivateKey() => _private;

        public RSA GetPublicKey() => _public;

        public string GetKeyId() => _keyId;

        private static string PemEncode(string label, byte[] data)
        {
            const int lineLength = 64;
            var base64 = Convert.ToBase64String(data);
            using var sw = new StringWriter();
            sw.WriteLine($"-----BEGIN {label}-----");
            for (int i = 0; i < base64.Length; i += lineLength)
            {
                sw.WriteLine(base64.Substring(i, Math.Min(lineLength, base64.Length - i)));
            }
            sw.WriteLine($"-----END {label}-----");
            return sw.ToString();
        }
    }
}
