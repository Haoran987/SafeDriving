using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotate : MonoBehaviour
{
    private Quaternion originalRotation;
    private bool isRotating = false; // locks input during rotation
    private float resetDelay = 1f;
    public InputActionProperty leftTrigger; // button6
    public InputActionProperty rightTrigger; // button5

    void OnEnable()
    {
        leftTrigger.action.Enable();
        rightTrigger.action.Enable();
    }

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {

        float leftTriggerValue = leftTrigger.action.ReadValue<float>();
        float rightTriggerValue = rightTrigger.action.ReadValue<float>();

        if (!isRotating)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || leftTriggerValue > 0.1f)
            {
                StartCoroutine(RotateAndReset(-55));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || rightTriggerValue > 0.1f)
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
