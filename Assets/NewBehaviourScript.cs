using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaulerMovement : MonoBehaviour
{
    public RigidBody rb;
    public Transform car;
    public float speed = 17;
    Vector3 forward = new Vector3(0,0,1);
    Vector3 backward = new Vector3(0,0,-1);

    void FixedUpdate()
    {
        if(Input.GetKey("Space")){
            rb.MovePosition(car.position + forward * speed * Time.deltaTime);
        }
        if(Input.GetKey("s")){
            rb.MovePosition(car.position + backward * speed * Time.deltaTime);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("This is a test!!!!");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("This is a test!!!!");
    }
}
