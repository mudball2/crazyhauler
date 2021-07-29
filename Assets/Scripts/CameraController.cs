using UnityEngine;
using System.Collections;
public class CameraController : MonoBehaviour {
    public Transform hauler;
    public float smoothing = 5f;
    Vector3 offSet;
    public float z;

    // Use this for initialization
    void Awake()
    {
        offSet = transform.position - hauler.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 camPos = hauler.position + offSet;
        camPos.z = camPos.z - z;
        transform.position = Vector3.Lerp(transform.position, camPos, smoothing * Time.deltaTime);

    }
}