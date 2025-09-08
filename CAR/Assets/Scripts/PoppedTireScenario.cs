using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PoppedTireScenario : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject instructionPanel;
    public GameObject endPanel;

    [Header("Scenario Elements")]
    public Transform popTriggerZone;

    // after 3 seconds, the instruction panel will be hidden
    private float instructionDelay = 3f;
    private float timer = 0f;

    

    IEnumerator PanelSetup()
    {
        // Show the instruction panel at the start
        instructionPanel.SetActive(true);
        yield return new WaitForSeconds(instructionDelay);
        instructionPanel.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PanelSetup());

        // Randomize the x position of the zone between -180 and 180
        float randomX = Random.Range(-180f, 180f);
        popTriggerZone.position = new Vector3(randomX, popTriggerZone.position.y, popTriggerZone.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            endPanel.SetActive(true);
            // Time.timeScale = 0f; // Pause the game
        }
    }

    public void RestartScenario()
    {
        SceneManager.LoadScene("Popped Tire");
    }

    public void ExitToScenarioSelect()
    {
        SceneManager.LoadScene("Scenario Select");
    }
}
