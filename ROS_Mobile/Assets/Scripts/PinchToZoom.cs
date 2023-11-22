using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchToZoom : MonoBehaviour
{
    public float zoomSpeed = 0.5f;
    public float minZoom = 1.0f;
    public float maxZoom = 5.0f;

    void Update()
    {
        /* MULTITOUCH FOR LATER
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevTouch0Pos = touch0.position - touch0.deltaPosition;
            Vector2 prevTouch1Pos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (prevTouch0Pos - prevTouch1Pos).magnitude;
            float touchDeltaMag = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Camera camera = Camera.main;

            float newOrthoSize = camera.orthographicSize + deltaMagnitudeDiff * zoomSpeed;
            newOrthoSize = Mathf.Clamp(newOrthoSize, minZoom, maxZoom);
            camera.orthographicSize = newOrthoSize;
        }
        */
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        // Check for the Shift key
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Camera camera = Camera.main;

            float newOrthoSize = camera.orthographicSize - zoomInput * zoomSpeed;
            newOrthoSize = Mathf.Clamp(newOrthoSize, minZoom, maxZoom);
            camera.orthographicSize = newOrthoSize;
        }

    }
}
