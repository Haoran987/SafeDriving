using UnityEngine;
using TMPro;
using EnvLoad;

public class Analysis : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI analysisText;

    private OpenAI _client;
    private EnvLoader _envLoader;

    private void Awake()
    {
        _envLoader = new EnvLoader();
        // Load env vars from StreamingAssets/.env
        EnvLoader.LoadOnce();

        var key = EnvLoader.Require("OPENAI_API_KEY");
        var model = EnvLoader.Get("OPENAI_MODEL", "gpt-4.1-mini");

        _client = new OpenAI(key, model);
    }

    private void Start()
    {
        StartCoroutine(_client.CreateResponse(
            "Looking at this data, please name 3 suggestions for what I can do to drive safer.",
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startDemo()
    {
        // After three seconds, hide statusText and show analysisText
        statusText.gameObject.SetActive(true);
        analysisText.gameObject.SetActive(false);
        Invoke("showAnalysis", 3f);
    }

    void showAnalysis()
    {
        statusText.gameObject.SetActive(false);
        analysisText.gameObject.SetActive(true);
    }
}
