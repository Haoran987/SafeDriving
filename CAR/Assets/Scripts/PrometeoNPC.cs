using UnityEngine;

public class PrometeoNPC : MonoBehaviour
{
    public float initialSpeed = 50f;
    public float speed = 50f; // km/h
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(-initialSpeed, 0f, 0f);
    }

    void FixedUpdate()
    {
        // Convert speed from km/h to m/s
        float targetSpeedMS = speed * 1000f / 3600f;

        // Current forward speed in m/s
        float currentSpeedMS = rb.linearVelocity.magnitude;

        if (currentSpeedMS < targetSpeedMS)
        {
            ApplyMotorTorque(200f); // Apply forward torque
        }
        else
        {
            ApplyMotorTorque(0f); // Stop accelerating
        }
    }

    void ApplyMotorTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
    }
}
