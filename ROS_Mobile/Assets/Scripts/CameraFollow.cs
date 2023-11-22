using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //public string drawingObjectName = "Drawing";
    public string pointCloudObjectName = "myPointCloud";
    public Transform target;
    public Vector3 offset;
    private bool hasFoundTarget = false; // Flag to track if the target has been found
    private bool printedWarning = false; // Flag to track if the warning has been printed
    
    private void Start()
    {
        // Initially set target to null
        target = null;
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        // Check if the target is null and hasn't been found yet
        if (target == null && !hasFoundTarget)
        {
            // Attempt to find the "PointCloud" GameObject by name
            GameObject pointCloudObject = GameObject.Find(pointCloudObjectName);

            if (pointCloudObject != null)
            {
                target = pointCloudObject.transform;
                hasFoundTarget = true; // Set the flag to true once the target is found
                UnityEngine.Debug.Log("PointCloud Game Object found.");
            }
            else
            {
                if (!printedWarning)
                { 
                    UnityEngine.Debug.LogWarning("PointCloud GameObject not found yet.");
                }
                // Only print the warning once 
                printedWarning = true;
            }
        }
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        
    }

    // Getter for the offset variable
    public Vector3 GetOffset()
    {
        return offset;
    }
}
