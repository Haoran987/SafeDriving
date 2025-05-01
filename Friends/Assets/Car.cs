using UnityEngine;

public class Car : MonoBehaviour
{

    public int speed = 40;
    public int rotationSpeed = 40;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        
        rb.AddRelativeForce(new Vector3(0,0,translation));
        rb.AddRelativeTorque(new Vector3(0, rotation, 0));


    }
}
