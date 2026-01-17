using UnityEngine;
using TMPro;

public class Analysis : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI analysisText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
