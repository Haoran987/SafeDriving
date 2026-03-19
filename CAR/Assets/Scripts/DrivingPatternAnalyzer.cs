using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using EnvLoad;

public class DrivingPatternAnalyzer : MonoBehaviour
{
    public enum AnalysisProvider
    {
        BackendProxy,
        OpenAI,
        Gemini
    }

    [Header("Provider")]
    public AnalysisProvider analysisProvider = AnalysisProvider.BackendProxy;

    [Header("Backend Proxy (Recommended)")]
    [Tooltip("Your server endpoint that calls OpenAI securely.")]
    public string analysisEndpoint = "http://localhost:8787/drivewise/analyze";

    [Header("OpenAI (Prototype Only)")]
    [Tooltip("If using OpenAI directly, the API key is loaded from StreamingAssets/.env via EnvLoader.")]
    public string openAIModel = "gpt-4o-mini";
    public bool useStructuredOutputs = true;

    [Header("Gemini (Prototype Only)")]
    [Tooltip("If using Gemini directly, the API key is loaded from StreamingAssets/.env via EnvLoader.")]
    public string geminiModel = "gemini-2.0-flash";
    public bool geminiUseJsonMode = true;

    [Header("Telemetry Source")]
    public PrometeoCarController prometeoCar;
    public Rigidbody carRigidbody;
    public water_etector waterDetector;

    [Header("Sampling")]
    [Tooltip("Sampling interval in seconds.")]
    public float sampleInterval = 0.1f;
    [Tooltip("Deceleration (m/s^2) below which a sample counts as a hard brake.")]
    public float hardBrakeDecel = 4.0f;
    [Tooltip("Absolute steer input above which a sample counts as a sharp steer.")]
    public float sharpSteerThreshold = 0.7f;

    [Header("Identity")]
    public string driverId = "anonymous";
    public string scenarioName = "Unknown";

    [Header("Events")]
    public StringEvent OnAnalysisJsonReady;

    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    [Serializable]
    public class SegmentSummary
    {
        public string driverId;
        public string scenario;
        public string segment;
        public float durationSec;
        public float distanceMeters;
        public int samples;
        public float avgSpeedMps;
        public float maxSpeedMps;
        public float avgSpeedKph;
        public float maxSpeedKph;
        public float avgThrottle;
        public float avgBrake;
        public float avgAbsSteer;
        public int hardBrakeCount;
        public int sharpSteerCount;
        public int collisionCount;
        public float timeInWaterSec;
    }

    [Serializable]
    public class SegmentPayload
    {
        public SegmentSummary summary;
        public string rawNotes;
    }

    private bool _isActive;
    private string _segmentName;
    private float _timeSinceSample;
    private float _segmentTimer;
    private float _distanceMeters;
    private float _sumSpeed;
    private float _sumThrottle;
    private float _sumBrake;
    private float _sumAbsSteer;
    private float _maxSpeed;
    private int _samples;
    private int _hardBrakes;
    private int _sharpSteers;
    private int _collisions;
    private float _timeInWater;
    private float _lastSpeed;
    private bool _haveLastSpeed;

    private EnvLoader _env;
    private string _apiKey;
    private string _geminiApiKey;

    private void Awake()
    {
        if (carRigidbody == null) carRigidbody = GetComponent<Rigidbody>();
        if (prometeoCar == null) prometeoCar = GetComponent<PrometeoCarController>();
        if (waterDetector == null) waterDetector = GetComponent<water_etector>();
    }

    private void Update()
    {
        if (!_isActive) return;

        _segmentTimer += Time.deltaTime;
        _timeSinceSample += Time.deltaTime;

        if (waterDetector != null && waterDetector.isInWater)
        {
            _timeInWater += Time.deltaTime;
        }

        if (_timeSinceSample >= sampleInterval)
        {
            SampleTelemetry(_timeSinceSample);
            _timeSinceSample = 0f;
        }
    }

    public void BeginSegment(string segmentName)
    {
        if (_isActive) return;

        _segmentName = string.IsNullOrWhiteSpace(segmentName) ? "Segment" : segmentName;
        _isActive = true;
        _timeSinceSample = 0f;
        _segmentTimer = 0f;
        _distanceMeters = 0f;
        _sumSpeed = 0f;
        _sumThrottle = 0f;
        _sumBrake = 0f;
        _sumAbsSteer = 0f;
        _maxSpeed = 0f;
        _samples = 0;
        _hardBrakes = 0;
        _sharpSteers = 0;
        _collisions = 0;
        _timeInWater = 0f;
        _haveLastSpeed = false;
    }

    public void EndSegment()
    {
        if (!_isActive) return;

        _isActive = false;
        var summary = BuildSummary();
        StartCoroutine(AnalyzeSummary(summary));
    }

    private void SampleTelemetry(float dt)
    {
        float speedMps = GetSpeedMps();
        float steer = GetSteerInput();
        float throttle = GetThrottleInput();
        float brake = GetBrakeInput();

        _distanceMeters += speedMps * dt;
        _sumSpeed += speedMps;
        _sumThrottle += Mathf.Clamp01(throttle);
        _sumBrake += Mathf.Clamp01(brake);
        _sumAbsSteer += Mathf.Abs(steer);
        _maxSpeed = Mathf.Max(_maxSpeed, speedMps);

        if (_haveLastSpeed)
        {
            float accel = (speedMps - _lastSpeed) / Mathf.Max(dt, 0.0001f);
            if (accel < -hardBrakeDecel) _hardBrakes++;
        }

        if (Mathf.Abs(steer) > sharpSteerThreshold) _sharpSteers++;

        _lastSpeed = speedMps;
        _haveLastSpeed = true;
        _samples++;
    }

    private float GetSpeedMps()
    {
        if (carRigidbody != null) return carRigidbody.linearVelocity.magnitude;
        if (prometeoCar != null) return prometeoCar.carSpeed / 3.6f;
        return 0f;
    }

    private float GetSteerInput()
    {
        if (prometeoCar != null && prometeoCar.steer.reference != null)
            return prometeoCar.steer.action.ReadValue<float>();

        return Input.GetAxis("Horizontal");
    }

    private float GetThrottleInput()
    {
        if (prometeoCar != null)
        {
            float throttle = prometeoCar.throttle.reference != null
                ? prometeoCar.throttle.action.ReadValue<float>()
                : 0f;
            float accelerator = prometeoCar.accelerator.reference != null
                ? prometeoCar.accelerator.action.ReadValue<float>()
                : 0f;
            return Mathf.Max(throttle, accelerator);
        }

        return Mathf.Max(0f, Input.GetAxis("Vertical"));
    }

    private float GetBrakeInput()
    {
        if (prometeoCar != null && prometeoCar.brake.reference != null)
            return Mathf.Max(0f, prometeoCar.brake.action.ReadValue<float>());

        float v = Input.GetAxis("Vertical");
        return Mathf.Max(0f, -v);
    }

    private SegmentSummary BuildSummary()
    {
        float avgSpeed = _samples > 0 ? _sumSpeed / _samples : 0f;
        float avgThrottle = _samples > 0 ? _sumThrottle / _samples : 0f;
        float avgBrake = _samples > 0 ? _sumBrake / _samples : 0f;
        float avgAbsSteer = _samples > 0 ? _sumAbsSteer / _samples : 0f;

        return new SegmentSummary
        {
            driverId = driverId,
            scenario = scenarioName,
            segment = _segmentName,
            durationSec = _segmentTimer,
            distanceMeters = _distanceMeters,
            samples = _samples,
            avgSpeedMps = avgSpeed,
            maxSpeedMps = _maxSpeed,
            avgSpeedKph = avgSpeed * 3.6f,
            maxSpeedKph = _maxSpeed * 3.6f,
            avgThrottle = avgThrottle,
            avgBrake = avgBrake,
            avgAbsSteer = avgAbsSteer,
            hardBrakeCount = _hardBrakes,
            sharpSteerCount = _sharpSteers,
            collisionCount = _collisions,
            timeInWaterSec = _timeInWater
        };
    }

    private IEnumerator AnalyzeSummary(SegmentSummary summary)
    {
        if (analysisProvider == AnalysisProvider.BackendProxy)
        {
            yield return SendToBackend(summary);
        }
        else if (analysisProvider == AnalysisProvider.OpenAI)
        {
            yield return SendToOpenAI(summary);
        }
        else
        {
            yield return SendToGemini(summary);
        }
    }

    private IEnumerator SendToBackend(SegmentSummary summary)
    {
        var payload = new SegmentPayload
        {
            summary = summary,
            rawNotes = "Generated by DrivingPatternAnalyzer"
        };

        string json = JsonUtility.ToJson(payload);
        using (var req = new UnityWebRequest(analysisEndpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            string response = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Analysis backend failed ({req.responseCode}): {req.error}\n{response}");
                yield break;
            }

            OnAnalysisJsonReady?.Invoke(response);
        }
    }

    private IEnumerator SendToOpenAI(SegmentSummary summary)
    {
        if (_env == null)
        {
            _env = new EnvLoader();
            _env.LoadOnce();
        }

        _apiKey = _env.Get("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            Debug.LogError("Missing OPENAI_API_KEY. Add it to StreamingAssets/.env.");
            yield break;
        }

        string prompt = BuildPrompt(summary);
        string body = BuildOpenAIRequest(openAIModel, prompt, useStructuredOutputs);

        using (var req = new UnityWebRequest("https://api.openai.com/v1/responses", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            yield return req.SendWebRequest();

            string response = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"OpenAI request failed ({req.responseCode}): {req.error}\n{response}");
                yield break;
            }

            // Try to extract text output for UI; fallback to raw JSON
            string outputText = OpenAI.TryExtractOutputText(response);
            OnAnalysisJsonReady?.Invoke(string.IsNullOrWhiteSpace(outputText) ? response : outputText);
        }
    }

    private IEnumerator SendToGemini(SegmentSummary summary)
    {
        if (_env == null)
        {
            _env = new EnvLoader();
            _env.LoadOnce();
        }

        _geminiApiKey = _env.Get("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(_geminiApiKey))
        {
            Debug.LogError("Missing GEMINI_API_KEY. Add it to StreamingAssets/.env.");
            yield break;
        }

        string prompt = BuildPrompt(summary);
        string body = BuildGeminiRequest(prompt, geminiUseJsonMode);

        string apiKeyParam = UnityWebRequest.EscapeURL(_geminiApiKey);
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={apiKeyParam}";
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            string response = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Gemini request failed ({req.responseCode}): {req.error}\n{response}");
                yield break;
            }

            string outputText = TryExtractGeminiText(response);
            OnAnalysisJsonReady?.Invoke(string.IsNullOrWhiteSpace(outputText) ? response : outputText);
        }
    }

    private static string BuildPrompt(SegmentSummary s)
    {
        // Keep the prompt concise; the summary JSON is the primary evidence.
        string summaryJson = JsonUtility.ToJson(s);
        return "You are a driving safety coach. Analyze the segment summary and return helpful, specific tips. "
             + "Prioritize actionable steps, and cite the evidence from the summary in each tip. "
             + "Summary JSON: " + summaryJson;
    }

    private static string BuildOpenAIRequest(string model, string prompt, bool structured)
    {
        string escapedPrompt = JsonEscape(prompt);

        if (!structured)
        {
            return "{"
                 + "\"model\":\"" + JsonEscape(model) + "\","
                 + "\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"" + escapedPrompt + "\"}]}]"
                 + "}";
        }

        // A compact JSON schema for structured, parseable tips.
        const string schema = "{"
            + "\"type\":\"object\","
            + "\"properties\":{"
            +   "\"summary\":{\"type\":\"string\"},"
            +   "\"issues\":{"
            +     "\"type\":\"array\","
            +     "\"items\":{"
            +       "\"type\":\"object\","
            +       "\"properties\":{"
            +         "\"issue\":{\"type\":\"string\"},"
            +         "\"evidence\":{\"type\":\"string\"},"
            +         "\"tip\":{\"type\":\"string\"}"
            +       "},"
            +       "\"required\":[\"issue\",\"evidence\",\"tip\"]"
            +     "}"
            +   "},"
            +   "\"scores\":{"
            +     "\"type\":\"object\","
            +     "\"properties\":{"
            +       "\"smoothness\":{\"type\":\"number\",\"minimum\":0,\"maximum\":100},"
            +       "\"hazard_response\":{\"type\":\"number\",\"minimum\":0,\"maximum\":100}"
            +     "},"
            +     "\"required\":[\"smoothness\",\"hazard_response\"]"
            +   "}"
            + "},"
            + "\"required\":[\"summary\",\"issues\",\"scores\"]"
            + "}";

        return "{"
             + "\"model\":\"" + JsonEscape(model) + "\","
             + "\"input\":[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"" + escapedPrompt + "\"}]}],"
             + "\"text\":{\"format\":{\"type\":\"json_schema\",\"strict\":true,\"schema\":" + schema + "}}"
             + "}";
    }

    private static string BuildGeminiRequest(string prompt, bool jsonMode)
    {
        string escapedPrompt = JsonEscape(prompt);
        if (!jsonMode)
        {
            return "{"
                 + "\"contents\":[{\"parts\":[{\"text\":\"" + escapedPrompt + "\"}]}]"
                 + "}";
        }

        return "{"
             + "\"contents\":[{\"parts\":[{\"text\":\"" + escapedPrompt + "\"}]}],"
             + "\"generationConfig\":{\"responseMimeType\":\"application/json\"}"
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

    [Serializable]
    private class GeminiResponse
    {
        public GeminiCandidate[] candidates;
    }

    [Serializable]
    private class GeminiCandidate
    {
        public GeminiContent content;
    }

    [Serializable]
    private class GeminiContent
    {
        public GeminiPart[] parts;
    }

    [Serializable]
    private class GeminiPart
    {
        public string text;
    }

    private static string TryExtractGeminiText(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            var parsed = JsonUtility.FromJson<GeminiResponse>(rawJson);
            if (parsed != null &&
                parsed.candidates != null &&
                parsed.candidates.Length > 0 &&
                parsed.candidates[0].content != null &&
                parsed.candidates[0].content.parts != null &&
                parsed.candidates[0].content.parts.Length > 0)
            {
                return parsed.candidates[0].content.parts[0].text;
            }
        }
        catch (Exception)
        {
            // Fall through to null
        }

        return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isActive) _collisions++;
    }
}

public class DrivingSegmentTrigger : MonoBehaviour
{
    public string segmentName = "Segment";
    public string playerTag = "Player";
    public bool endOnExit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var analyzer = other.GetComponentInParent<DrivingPatternAnalyzer>();
        if (analyzer != null)
        {
            analyzer.BeginSegment(segmentName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!endOnExit) return;
        if (!other.CompareTag(playerTag)) return;

        var analyzer = other.GetComponentInParent<DrivingPatternAnalyzer>();
        if (analyzer != null)
        {
            analyzer.EndSegment();
        }
    }
}
