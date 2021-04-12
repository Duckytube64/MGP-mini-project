using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 mouseDelta = Vector3.zero;
    private Vector3 lastMousePosition = Vector3.zero;
    float speed = 10.0f;
    Transform cam, mesh;

    void Start()
    {
        cam = transform.Find("Main Camera");
        mesh = transform.Find("ProcPlane");
    }

    void Update()
    {
        mouseDelta = lastMousePosition - Input.mousePosition;
        lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.down, mouseDelta.x * speed * Time.deltaTime);
        }
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(Vector3.left, -mouseDelta.y * speed * Time.deltaTime);
        }
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount != 0)
        {
            cam.position += cam.forward * scrollAmount * 5 * speed;
        }
        cam.transform.LookAt(mesh);
    }
}
