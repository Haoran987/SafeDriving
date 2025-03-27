using UnityEngine;

public class Car : MonoBehaviour
{

    public int speed = 40;
    public int rotationSpeed = 40;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        
        transform.Translate(0,0, translation);
        transform.Rotate(0, rotation, 0);


    }
}
