using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera mainCamera; // Reference to the Main Camera
    public Camera birdseyeCamera; // Reference to the Birdseye Camera
    public KeyCode switchKey = KeyCode.C; // The key to toggle between cameras

    private Camera activeCamera; // The currently active camera

    void Start()
    {
        // Initialize with the Main Camera as the active camera
        SetActiveCamera(mainCamera);
    }

    void Update()
    {
        // Check if the switch key is pressed
        if (Input.GetKeyDown(switchKey))
        {
            // Toggle between cameras
            if (activeCamera == mainCamera)
            {
                SetActiveCamera(birdseyeCamera);
            }
            else
            {
                SetActiveCamera(mainCamera);
            }
        }
    }

    // Function to set the active camera and disable the other one
    private void SetActiveCamera(Camera newActiveCamera)
    {
        // Disable the previously active camera
        if (activeCamera != null)
        {
            activeCamera.gameObject.SetActive(false);
        }

        // Enable the new active camera
        newActiveCamera.gameObject.SetActive(true);

        // Change the tag to "MainCamera" so that CameraFollow.cs can find it
        newActiveCamera.tag = "MainCamera";

        // The other camera should get the "Untagged" tag
        (mainCamera.tag, birdseyeCamera.tag) = (newActiveCamera == mainCamera) ? ("MainCamera", "SecondCamera") : ("SecondCamera", "MainCamera");

        // Update the active camera reference
        activeCamera = newActiveCamera;


  
    }
}
