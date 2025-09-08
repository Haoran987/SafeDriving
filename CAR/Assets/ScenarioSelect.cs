using UnityEngine;

public class ScenarioSelect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadPoppedTireScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Popped Tire");
    }

    public void LoadTBoneScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("T-Bone");
    }

    public void LoadHydroplaningScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hydroplaning");
    }

    public void LoadHighwayScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Highway");
    }

    public void BackToTitleScreen()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }
}
