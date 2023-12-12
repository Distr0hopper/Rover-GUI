using UnityEngine;



public class MouseClickCoordinatesTestScene : MonoBehaviour
{

    Vector3 mousePositionScreen;
    Vector3 mousePositionWorld;
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
            Debug.DrawLine(ray.origin, hit.point, Color.green, 5.0f);
            //Debug.LogWarning(hit.collider.gameObject.name);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 5.0f);
            Debug.LogError("No hit");
        }

        // Transform game object position to mouse position, for visualization purposes 0.01 above ground!
        //transform.position = mousePositionWorld;
        //transform.position = new Vector3(mousePositionWorld.x, mousePositionWorld.y + 0.09f, mousePositionWorld.z);
        transform.position = new Vector3(mousePositionWorld.x, -0.4f, mousePositionWorld.z);

        //Debug.Log("Mouse Clicked at Screen Position: " + mousePositionScreen);
        //Debug.Log("Mouse Clicked at World Position: " + mousePositionWorld);

        boolClicked = true;
    }
}
