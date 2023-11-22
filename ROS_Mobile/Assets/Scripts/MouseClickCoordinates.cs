using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MouseClickCoordinates : MonoBehaviour
{

    public Vector3 mousePositionScreen;
    public Vector3 mousePositionWorld;
    private static bool boolClicked = false;

    public float doubleClickTimeThreshold = 0.2f;
    private float lastClickTime = 0f;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {   
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick <= doubleClickTimeThreshold)
            {
                onDoubleClicked();
                lastClickTime = 0f;
            } else
            {
                lastClickTime = Time.time;
            }
        }
    }

    // Getter for the boolClicked variable
    public static bool GetBoolClicked()
    {
        return boolClicked;
    }

    // Setter for the boolClicked variable
    public static void SetBoolClicked(bool value)
    {
        boolClicked = value;
    }

    void onDoubleClicked()
    {
        // Get the mouse position in screen coordinates
        mousePositionScreen = Input.mousePosition;

        // Ray from screen position into world
        Ray ray = Camera.main.ScreenPointToRay(mousePositionScreen);

        // See if it hits something
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Get the point where it hit
            mousePositionWorld = hit.point;

            Debug.LogWarning(hit.collider.gameObject.name);
        }
        else
        {
            Debug.LogError("No hit");
        }

        // Transform game object position to mouse position, for visualization purposes 0.01 above ground!
        //transform.position = mousePositionWorld;
        transform.position = new Vector3(mousePositionWorld.x, mousePositionWorld.y + 0.01f, mousePositionWorld.z);

        Debug.Log("Mouse Clicked at Screen Position: " + mousePositionScreen);
        Debug.Log("Mouse Clicked at World Position: " + mousePositionWorld);

        boolClicked = true;
    }
}
