using UnityEngine;

public class CarSpawn : MonoBehaviour
{
    public GameObject carPrefab; // Reference to the car prefab
    public Transform spawnPoint; // Reference to the spawn point
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC")) {
            Transform parentTransform = other.transform.parent.parent;
            parentTransform.position = spawnPoint.position;
        }
    }
}
