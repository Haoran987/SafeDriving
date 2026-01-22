using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OpenAI: Monobehaviour
{
    // Load API Key


    // Properties
    List<List<int>> data = [];

    // Methods
    
    // Send data to API
    IEnumerator GetAnalysis()
    {
        UnityWebRequest www = UnityRequest.Post("https://api.openai.com/v1", "{}", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Form Upload Complete!");
        }
    }

    // 
}