using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    private Quaternion originalRotation;
    private bool isRotating = false; // locks input during rotation
    private float resetDelay = 1f;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (!isRotating)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(RotateAndReset(-55));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(RotateAndReset(55));
            }
        }
    }

    System.Collections.IEnumerator RotateAndReset(float yAngle)
    {
        isRotating = true;

        // Apply rotation instantly
        transform.Rotate(0, yAngle, 0);

        // Wait for 1 second
        yield return new WaitForSeconds(resetDelay);

        // Reset rotation to original
        transform.localRotation = Quaternion.Euler(Vector3.zero);

        isRotating = false;
    }
}
