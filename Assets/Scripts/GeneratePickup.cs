using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePickup : MonoBehaviour
{
    public GameObject car; 
    private int xPos;
    private int zPos;
    public GameObject dest; 
    public bool deliveryExists;
    public bool carExists;

    void Update () {
        if (!carExists && !deliveryExists) 
        {
            carExists = true;
            CarDrop();
        }
        if(deliveryExists && !carExists)
        {
            DestDrop();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("hauler") && carExists)
        {
            deliveryExists = true;
            carExists = false;
            Destroy(car);
        }
        else if (col.gameObject.CompareTag("hauler") && deliveryExists)
        {
            Destroy(dest);
            deliveryExists = false;
            //Score
        }
    }

    void CarDrop()
    {
        xPos = Random.Range(4, 50);
        zPos = Random.Range(-22, 22);
        Instantiate(car, new Vector3(xPos, -0.75f, zPos), Quaternion.identity);
    }

    void DestDrop()
    {
        xPos = Random.Range(4, 50);
        zPos = Random.Range(29, 60);
        Instantiate(dest, new Vector3(xPos, 1.2f, zPos), Quaternion.identity);
    }
}
