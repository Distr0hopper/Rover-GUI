using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFollow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosition = Camera.main.transform.position;

        /*  Follow the camera on the x and z axis, but the y axis is in relation to the laserscan (but not the roll-angle)!
         * 
        float cameraOffset = new CameraFollow().GetOffset().y;
        // Make sure the ground follows the camera, but stays on the ground (substract the offset of 50)
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y - cameraOffset, cameraPosition.z);
        */ 

        // Follow the camera on the x and z axis, but stay on the ground 
        transform.position = new Vector3(cameraPosition.x, 0, cameraPosition.z);
    }
}
