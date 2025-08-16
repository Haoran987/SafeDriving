using UnityEngine;

public class PrometeoNPC : MonoBehaviour
{
    public float initialSpeed = 50f;
    public float speed = 50f; // km/h
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public PrometeoCarController playerCar; // reference to the player car

    private Rigidbody rb;
    private bool npcStarted = false;
    private float moveTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero; // stay still
        ApplyMotorTorque(0f);
        ApplyBrake(10000f); // hold NPC in place with big brakes
    }

    void FixedUpdate()
    {
        if (!npcStarted)
        {
            if (playerCar != null && playerCar.carSpeed > 0.5f) // player moving
            {
                moveTimer += Time.fixedDeltaTime;
                if (moveTimer >= 0.75f)
                {
                    npcStarted = true;

                    // release brakes
                    ApplyBrake(0f);

                    // give initial push
                    rb.linearVelocity = new Vector3(-initialSpeed * 1000f / 3600f, 0f, 0f);
                }
            }
            else
            {
                moveTimer = 0f; // reset if player stops early
            }
            return;
        }

        // NPC driving logic
        float targetSpeedMS = speed * 1000f / 3600f;
        float currentSpeedMS = rb.linearVelocity.magnitude;

        if (currentSpeedMS < targetSpeedMS)
        {
            ApplyMotorTorque(200f);
        }
        else
        {
            ApplyMotorTorque(0f);
        }
    }

    void ApplyMotorTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
    }

    void ApplyBrake(float torque)
    {
        frontLeftCollider.brakeTorque = torque;
        frontRightCollider.brakeTorque = torque;
        rearLeftCollider.brakeTorque = torque;
        rearRightCollider.brakeTorque = torque;
    }
}
