using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    private int distance = 100;
    private int width;
    private int height;

    void Start()
    {
        transform.LookAt(target);
        width = Screen.width;
        height = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = Input.mousePosition;
        float speed = 10.0f;
        transform.Rotate(Vector3.down, 10.0f * Time.deltaTime);
        Vector3 pivotPoint = new Vector3(128.0f, 0.0f, 128.0f);
        if (Input.mousePosition.x > width - 10)
        {
            transform.RotateAround(pivotPoint, Vector3.up, Mathf.Abs(mousePosition.x - distance) / distance);
        }

        if (Input.mousePosition.x < 0 + 10)
        {
            transform.RotateAround(pivotPoint, Vector3.up, -Mathf.Abs(mousePosition.x - distance) / distance);
        }

        if (Input.GetMouseButton(1))
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed,
                                           0.0f, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed);
            }

            else if (Input.GetAxis("Mouse X") < 0)
            {
                transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed,
                                           0.0f, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed);
            }
        }
    }
}
