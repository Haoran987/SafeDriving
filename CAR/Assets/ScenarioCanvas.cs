using UnityEngine;
using UnityEngine.Events;

public class ScenarioCanvas : MonoBehaviour
{
    public PrometeoCarController prometeoCarController;
    public GameObject deathScreenUI;
    public GameObject instructionUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prometeoCarController.OnPlayerDeath.AddListener(ShowDeathScreen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowDeathScreen()
    {
        // Show the death screen UI
        deathScreenUI.SetActive(true);
    }

    public void RestartCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }
}
