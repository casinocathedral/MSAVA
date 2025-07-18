using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace M_SAVA_INF.Environment
{
    public class Environment
    {
        private readonly Dictionary<string, string> _values;
        private static readonly string EnvFolder = Path.GetDirectoryName(typeof(Environment).Assembly.Location)!;
        private static readonly string EnvFileName = IsDevelopment() ? ".env.development" : ".env";
        private static readonly string EnvFilePath = Path.Combine(EnvFolder, EnvFileName);

        public static Environment Instance { get; } = new Environment();

        private Environment()
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

        public string? GetValue(string key)
        {
            _values.TryGetValue(key, out var value);
            return value;
        }

        public byte[] GetSigningKeyBytes()
        {
            var key = GetValue("issuer_signing_key");
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException($"issuer_signing_key is missing in {EnvFileName}");
            return Encoding.UTF8.GetBytes(key);
        }
    }
}
