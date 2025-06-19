using UnityEngine;

public class water_etector : MonoBehaviour
{
    [HideInInspector] public bool isInWater = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = false;
    }
}
