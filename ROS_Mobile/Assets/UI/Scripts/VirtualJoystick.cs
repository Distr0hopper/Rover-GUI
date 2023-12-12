using myUIController;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class VirtualJoystick : MonoBehaviour
{
    private VisualElement m_JoystickBack;
    private VisualElement m_JoystickHandle;
    private Vector2 m_JoystickPointerDownPosition;
    private Vector2 m_JoystickDelta; // Between -1 and 1

    //public Rigidbody worldObjectToMove;
    [FormerlySerializedAs("worldObjectPushStrength")] public float rotationSpeed = 1.25f;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_JoystickBack = root.Q("JoystickBackground");
        m_JoystickHandle = root.Q("Joystick");
        m_JoystickHandle.RegisterCallback<PointerDownEvent>(OnPointerDown);
        m_JoystickHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
        m_JoystickHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    void Update()
    {
        Camera _camera = Camera.main;
        if (m_JoystickDelta != Vector2.zero)
        {

            Debug.Log(_camera.name);
            float rotationX = -m_JoystickDelta.y * rotationSpeed;
            float rotationY = m_JoystickDelta.x * rotationSpeed;
            float rotationZ = 0;

            Quaternion rotationQuat = Quaternion.Euler(rotationX, rotationY, rotationZ);
            //Rotate the main camera
            if (_camera.name.Equals("Main Camera"))
            {
                Quaternion deltaT = Quaternion.AngleAxis(rotationY, Vector3.forward);
                _camera.transform.rotation *= deltaT;
            }
            else
                _camera.transform.rotation *= rotationQuat;
                Vector3 currentRotation = _camera.transform.eulerAngles;
                // Set the z euler angle of transformation to 0
                currentRotation.z = 0; // Lock the Z-axis rotation
                _camera.transform.eulerAngles = currentRotation;
        }
        //worldObjectToMove.AddForce(new Vector3(m_JoystickDelta.x, 0, -m_JoystickDelta.y) * worldObjectPushStrength);
    }

    void OnPointerDown(PointerDownEvent e)
    {
        m_JoystickHandle.CapturePointer(e.pointerId);
        m_JoystickPointerDownPosition = e.position;
    }

    void OnPointerUp(PointerUpEvent e)
    {
        m_JoystickHandle.ReleasePointer(e.pointerId);
        m_JoystickHandle.transform.position = Vector3.zero;
        m_JoystickDelta = Vector2.zero;
    }

    void OnPointerMove(PointerMoveEvent e)
    {
        if (!m_JoystickHandle.HasPointerCapture(e.pointerId))
            return;
        var pointerCurrentPosition = (Vector2) e.position;
        var pointerMaxDelta = (m_JoystickBack.worldBound.size - m_JoystickHandle.worldBound.size) / 2;
        var pointerDelta = Clamp(pointerCurrentPosition - m_JoystickPointerDownPosition, -pointerMaxDelta,
            pointerMaxDelta);
        m_JoystickHandle.transform.position = pointerDelta;
        m_JoystickDelta = pointerDelta / pointerMaxDelta;
    }

    static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max) =>
        new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
}
