using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioSelect : MonoBehaviour
{
    // Loads the popped tire driving scenario
    public void LoadPoppedTireScenario()
    {
        SceneManager.LoadScene("Popped Tire");
    }

    // Loads the T-Bone driving scenario
    public void LoadTBoneScenario()
    {
        SceneManager.LoadScene("T-Bone");
    }

    // Loads the hydroplaning driving scenario
    public void LoadHydroplaningScenario()
    {
        SceneManager.LoadScene("New Hydroplane");
    }

    // Loads the highway driving scenario
    public void LoadHighwayScenario()
    {
        SceneManager.LoadScene("Highway");
    }

    // Loads the tutorial / guide scene for hydroplaning
    public void LoadHydrGuideScene()
    {
        Debug.Log("Loading Hydroguide...");
    SceneManager.LoadScene("Hydroguide");
        SceneManager.LoadScene("Hydroguide");
    }

    // Returns to the title screen
    public void BackToTitleScreen()
    {
        SceneManager.LoadScene("Title Screen");
    }
}
