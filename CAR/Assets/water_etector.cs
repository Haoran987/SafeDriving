using UnityEngine;

public class water_etector : MonoBehaviour
{
    [HideInInspector] public bool isInWater = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = true;
        Debug.Log("Hi");
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water")) isInWater = false;
        Debug.Log("Bye");
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Water")) isInWater = true;
        Debug.Log("Hi");
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Water")) isInWater = true;
        Debug.Log("Hi");
    }
}
