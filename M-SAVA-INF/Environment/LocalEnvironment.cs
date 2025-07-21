using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace M_SAVA_INF.Environment
{
    public class LocalEnvironment : ILocalEnvironment
    {
        private readonly Dictionary<string, string> _values;
        private static readonly string EnvFolder = Path.GetDirectoryName(typeof(LocalEnvironment).Assembly.Location)!;
        private static readonly string EnvFileName = IsDevelopment() ? ".env.development" : ".env";
        private static readonly string EnvFilePath = Path.Combine(EnvFolder, EnvFileName);

        public static LocalEnvironment Instance { get; } = new LocalEnvironment();

        public LocalEnvironment()
        {
            _values = LoadEnvFile(EnvFilePath);
        }

        public static bool IsDevelopment()
        {
            var aspnetEnv = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Equals(aspnetEnv, "Development", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> LoadEnvFile(string path)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
                return dict;
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;
                var idx = trimmed.IndexOf('=');
                if (idx <= 0) continue;
                var key = trimmed.Substring(0, idx).Trim();
                var value = trimmed.Substring(idx + 1).Trim();
                dict[key] = value;
            }
            return dict;
        }

        public string GetValue(string key)
        {
            if (_values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;

            throw new KeyNotFoundException($"Environment variable '{key}' is not set or is empty in {EnvFileName}");
        }

        public byte[] GetSigningKeyBytes()
        {
            string key = GetValue("issuer_signing_key");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException($"'issuer_signing_key' is missing or empty in {EnvFileName}");
            return Encoding.UTF8.GetBytes(key);
        }
    }
}
