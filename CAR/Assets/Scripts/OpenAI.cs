using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
/*

    private OpenAI _client;

    private void Awake()
    {
        // Load env vars from StreamingAssets/.env
        EnvLoader.LoadOnce();

        var key = EnvLoader.Require("OPENAI_API_KEY");
        var model = EnvLoader.Get("OPENAI_MODEL", "gpt-4.1-mini");

        _client = new OpenAI(key, model);
    }

    private void Start()
    {
        StartCoroutine(_client.CreateResponse(
            "Hello world.",
            onSuccess: (rawJson) =>
            {
                Debug.Log("RAW JSON:\n" + rawJson);

                var text = OpenAI.TryExtractOutputText(rawJson);
                if (!string.IsNullOrEmpty(text))
                    Debug.Log("OUTPUT TEXT:\n" + text);
            },
            onError: (err) => Debug.LogError(err)
        ));
    }
*/
public class OpenAI
{
    private readonly string _apiKey;
    private readonly string _model;
    private const string Endpoint = "https://api.openai.com/v1/responses";

    public OpenAI(string apiKey, string model = "gpt-4.1-mini")
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = string.IsNullOrWhiteSpace(model) ? "gpt-4.1-mini" : model;
    }

    /// <summary>
    /// Fire-and-forget style coroutine call.
    /// onSuccess gets raw JSON response. onError gets a readable message.
    /// </summary>
    public IEnumerator CreateResponse(string userPrompt, Action<string> onSuccess, Action<string> onError = null)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            onError?.Invoke("Prompt was empty.");
            yield break;
        }

        var bodyJson = BuildJson(_model, userPrompt);

        using (var req = new UnityWebRequest(Endpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            yield return req.SendWebRequest();

            var text = req.downloadHandler.text;

            if (req.result != UnityWebRequest.Result.Success)
            {
                var msg = $"OpenAI request failed ({req.responseCode}): {req.error}\n{text}";
                onError?.Invoke(msg);
                yield break;
            }

            onSuccess?.Invoke(text);
        }
    }

    // Minimal JSON build (no external JSON package needed)
    private static string BuildJson(string model, string prompt)
    {
        return "{"
               + "\"model\":\"" + JsonEscape(model) + "\","
               + "\"input\":["
               + "  {\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"" + JsonEscape(prompt) + "\"}]}"
               + "]"
               + "}";
    }

    private static string JsonEscape(string s)
    {
        if (s == null) return "";
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// Optional: a very lightweight extraction for "output_text" if present.
    /// This is NOT a full JSON parser, but often works for PoCs.
    /// For serious parsing, use Newtonsoft JSON or Unity's JsonUtility with a proper model.
    /// </summary>
    public static string TryExtractOutputText(string rawJson)
    {
        if (string.IsNullOrEmpty(rawJson)) return null;

        // Look for "output_text":"..."
        var key = "\"output_text\"";
        var idx = rawJson.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return null;

        // Find the first quote after the colon
        idx = rawJson.IndexOf(':', idx);
        if (idx < 0) return null;

        idx = rawJson.IndexOf('\"', idx);
        if (idx < 0) return null;

        idx++; // start of value

        var sb = new StringBuilder();
        bool escape = false;

        for (int i = idx; i < rawJson.Length; i++)
        {
            char c = rawJson[i];
            if (escape)
            {
                // handle a few escapes
                sb.Append(c switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '"' => '"',
                    '\\' => '\\',
                    _ => c
                });
                escape = false;
                continue;
            }

            if (c == '\\')
            {
                escape = true;
                continue;
            }

            if (c == '"')
                return sb.ToString();

            sb.Append(c);
        }

        return null;
    }
}
