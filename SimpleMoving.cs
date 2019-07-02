using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoving : MonoBehaviour {
    public float speed = 1f;
    public float sensitivity = 1f;
    private float h=0f;
    private float v=0f;
    void Update () {
        if (Input.GetKey(KeyCode.W)) gameObject.transform.position += transform.forward*speed;
        if (Input.GetKey(KeyCode.S)) gameObject.transform.position += -transform.forward*speed;
        if (Input.GetKey(KeyCode.A)) gameObject.transform.position += -transform.right*speed;
        if (Input.GetKey(KeyCode.D)) gameObject.transform.position += transform.right*speed;
        if (Input.GetKey(KeyCode.Mouse2))
        {
            v -= Input.GetAxis("Mouse Y") * sensitivity;
            v = Mathf.Clamp(v, -90f, 90f);
            h += Input.GetAxis("Mouse X") * sensitivity;
            transform.eulerAngles = new Vector3(v, h, 0f);
        }
    }
}
