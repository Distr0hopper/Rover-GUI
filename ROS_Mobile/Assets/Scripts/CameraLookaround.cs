using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookaround : MonoBehaviour
{
    public float rotationSpeed = 0.1f;
    void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);

            // Calculate rotation angles based on touch delta position
            if (touch.phase == TouchPhase.Moved)
            {
                float rotationX = -touch.deltaPosition.y * rotationSpeed;
                float rotationY = touch.deltaPosition.x * rotationSpeed;
                float rotationZ = 0;

                Quaternion rotationQuat = Quaternion.Euler(rotationX, rotationY, rotationZ);

                GameObject birdseyeCamera = GameObject.Find("Birdseye Camera");

                //allow only rotation around the y axis
                if (birdseyeCamera != null && birdseyeCamera.activeSelf)
                {
                    Quaternion deltaT = Quaternion.AngleAxis(rotationY, Vector3.forward);
                    transform.rotation *= deltaT;
                }
                else transform.rotation *= rotationQuat;
                Vector3 currentRotation = transform.eulerAngles;
                // Set the z euler angle of transformation to 0
                currentRotation.z = 0; // Lock the Z-axis rotation
                transform.eulerAngles = currentRotation;
            }
        }
    }
}
