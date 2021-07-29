using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hauler : MonoBehaviour
{
    //public Rigidbody rb;
   // public Transform car;
    //public float speed = 17;
    //Vector3 forward = new Vector3(0,0,1);
    //Vector3 backward = new Vector3(0,0,-1);
    public float currentSpeed = 0.0f;
    private float acceleration = 20.0f;
    private float deceleration = 25.0f;
    public float topSpeed = 100f;
    public bool accelerate = true;

    void FixedUpdate()
    {
        if(Input.GetKey("space")){
            if(currentSpeed < topSpeed)
            {
                currentSpeed += (acceleration * Time.deltaTime);
            }
        }
        else if (currentSpeed > 0)
        {
            currentSpeed -= (deceleration * Time.deltaTime);
        }
        //rb.MovePosition(car.position + forward * speed * Time.deltaTime);
        //steer left and right
        if(Input.GetKey("a"))
        {
            transform.Rotate(0, -5.0f, 0);
        }
        if (Input.GetKey("d"))
        {
            transform.Rotate(0, 5.0f, 0);
        }
        if(Input.GetKey("s"))
        {
            deceleration = 75.0f;
        }
        else
        {
            deceleration = 25.0f;
        }

        if (accelerate)
        {
            transform.Translate((Vector3.forward) * currentSpeed * Time.deltaTime);
        }
        //...move backward if 'R' gear
        else
        {
            transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("This is a test!!!!");
    }

}
