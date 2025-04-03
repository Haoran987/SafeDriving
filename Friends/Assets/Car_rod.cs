using UnityEngine;

public class Car_rod : MonoBehaviour
{
    public GameObject frontLeftWheel;
    public GameObject frontRightWheel;
    public GameObject backLeftWheel;
    public GameObject backRightWheel;

    public float speed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check for input (GetAxis)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Move the car forward/backward by spinning the wheels via rb torque
        if (verticalInput != 0)
        {
            frontLeftWheel.GetComponent<Rigidbody>().AddTorque(transform.up * verticalInput * speed);
            frontRightWheel.GetComponent<Rigidbody>().AddTorque(transform.up * verticalInput * speed);
        }

    }
}
