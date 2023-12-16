using myUIController;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    #region UI Elements
    private VisualElement m_JoystickBack;
    private VisualElement m_JoystickHandle;

    private Button m_inreaseFOV;
    private Button m_decreaseFOV;
    
    #endregion
    
    
    private Vector2 m_JoystickPointerDownPosition;
    private Vector2 m_JoystickDelta; // Between -1 and 1
    
    public string pointCloudObjectName = "myPointCloud";
    private Transform target = null;
    public Vector3 offset;
    private bool hasFoundTarget; // Flag to track if the target has been found
    
    private Camera _camera;
    
    [FormerlySerializedAs("worldObjectPushStrength")] public float rotationSpeed = 1.25f;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_JoystickBack = root.Q("JoystickBackground");
        m_JoystickHandle = root.Q("Joystick");
        m_inreaseFOV = root.Q<Button>("IncreaseFOV");
        m_decreaseFOV = root.Q<Button>("DecreaseFOV");
        m_JoystickHandle.RegisterCallback<PointerDownEvent>(OnPointerDown);
        m_JoystickHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
        m_JoystickHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        m_inreaseFOV.clicked += () => { ChangeFOV(5); };
        m_decreaseFOV.clicked += () => { ChangeFOV(-5); };
    }

    void Update()
    {
        _camera = Camera.main;
        
        // Check if the target is null and hasn't been found yet
        if (target == null && !hasFoundTarget)
        {
            // Attempt to find the "PointCloud" GameObject by name
            GameObject pointCloudObject = GameObject.Find(pointCloudObjectName);

            if (pointCloudObject != null)
            {
                target = pointCloudObject.transform;
                hasFoundTarget = true; // Set the flag to true once the target is found
            }
        }
        if (target != null)
        {
            _camera.transform.position = target.position + offset;
        }
        
        if (inputDetected())
        {
            var rotationY = getJoystickInput(out var rotationQuat);
            // Cameras are rotated differently since Main Camera (Birdseye view) is a orthographic camera and the other camera is perspective
            if (_camera.name.Equals("Main Camera"))
            {
                Quaternion deltaT = Quaternion.AngleAxis(rotationY, Vector3.forward);
                _camera.transform.rotation *= deltaT;
            }
            else
            {
                _camera.transform.rotation *= rotationQuat;
            }
                rotateCamera();
        }
        
    }

    private void rotateCamera()
    {
        Vector3 currentRotation = _camera.transform.eulerAngles;
        // Set the z euler angle of transformation to 0
        currentRotation.z = 0; // Lock the Z-axis rotation
        _camera.transform.eulerAngles = currentRotation;
    }

    private float getJoystickInput(out Quaternion rotationQuat)
    {
        float rotationX = -m_JoystickDelta.y * rotationSpeed;
        float rotationY = m_JoystickDelta.x * rotationSpeed;
        float rotationZ = 0;

        rotationQuat = Quaternion.Euler(rotationX, rotationY, rotationZ);
        return rotationY;
    }

    private bool inputDetected()
    {
        return m_JoystickDelta != Vector2.zero;
    }

    void ChangeFOV(int number)
    {
        // Birdseye Camera is Main Camera, if its Birdseye just change the y position since its a orthographic camera without FOV. Else change the FOV from perspective camera
        if (_camera.name.Equals("Main Camera"))
        {
            float numberDouble = number < 0 ? -0.5f : 0.5f;
            offset.y += numberDouble;
            //_camera.transform.position =  new Vector3(_camera.transform.position.x, _camera.transform.position.y + number, _camera.transform.position.z);
        }
        else
        {
            _camera.fieldOfView += number;
        }
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
