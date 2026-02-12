using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EnvLoad {
public class EnvLoader
{
    private Dictionary<string, string> _vars;

    public void LoadOnce(string fileName = ".env")
    {
        if (_vars != null) return;

        _vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"EnvLoader: .env not found at: {path}");
            return;
        }

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("#")) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line.Substring(0, idx).Trim();
            var value = line.Substring(idx + 1).Trim();

            // Strip optional quotes
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            _vars[key] = value;
        }
    }

    public string Get(string key, string fallback = null)
    {
        if (_vars == null) LoadOnce();
        return (_vars != null && _vars.TryGetValue(key, out var v)) ? v : fallback;
    }

    public string Require(string key)
    {
        var v = Get(key);
        if (string.IsNullOrEmpty(v))
            throw new Exception($"Missing required env var: {key} (check StreamingAssets/.env)");
        return v;
    }
}
}