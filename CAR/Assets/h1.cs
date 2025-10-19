using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader1 : MonoBehaviour
{
    public void LoadGuideScene()
    {
        SceneManager.LoadScene("Hydroguide");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Scenario Select");
    }
}
