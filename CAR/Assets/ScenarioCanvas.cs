using UnityEngine;

public class ScenarioCanvas : MonoBehaviour
{
    public PrometeoCarController prometeoCarController;
    public GameObject deathScreenUI;
    public GameObject passUI;
    public GameObject instructionUI;

    void Start()
    {
        if (prometeoCarController == null)
        {
            prometeoCarController = FindObjectOfType<PrometeoCarController>();
        }

        if (prometeoCarController != null)
        {
            prometeoCarController.OnPlayerDeath.AddListener(ShowDeathScreen);
            prometeoCarController.OnPlayerPass.AddListener(Showps);

        }

        else
        {
            Debug.LogError("❌ No PrometeoCarController found in scene!");
        }
    }

    void ShowDeathScreen()
    {
        deathScreenUI.SetActive(true);
    }
    void Showps()
    {
        passUI.SetActive(true);
    }

    public void RestartCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }
}
