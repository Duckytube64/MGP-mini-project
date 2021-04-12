using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    private int distance = 100;

    void Start()
    {
        transform.LookAt(target);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 100.0f;
        // transform.Rotate(Vector3.down, 10.0f * Time.deltaTime);

        if (Input.GetMouseButton(0))
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                transform.Rotate(Vector3.down, speed * Time.deltaTime);
            }

            else if (Input.GetAxis("Mouse X") < 0)
            {
                transform.Rotate(Vector3.down, - speed * Time.deltaTime);
            }
        }
        if (Input.GetMouseButton(1))
        {
            if (Input.GetAxis("Mouse Y") > 0)
            {
                transform.Rotate(Vector3.left, speed * Time.deltaTime);
            }

            else if (Input.GetAxis("Mouse Y") < 0)
            {
                transform.Rotate(Vector3.left, -speed * Time.deltaTime);
            }
        }
    }
}
