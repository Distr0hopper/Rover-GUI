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
    
    [FormerlySerializedAs("pointCloudObjectName")] public string followingObject = "myPointCloud";
    private Transform target = null;
    [FormerlySerializedAs("offset")] public Vector3 mainCamOffset;
    public Vector3 secondCamOffset;
    private bool hasFoundTarget; // Flag to track if the target has been found
    private bool _isMainViewActive = true;
    
  
    private Camera activeMainUICamera { get; set; }
    private Camera mainCamera;
    private Camera secondCamera;
    
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
        m_decreaseFOV.clicked += () => { ChangeFOV(5); }; // If you press FOV + you want to "zoom in"
        m_inreaseFOV.clicked += () => { ChangeFOV(-5); }; // If you press FOV - you want to "zoom out"
    }

    void Start()
    {
        foreach (var camera in Camera.allCameras)
        {
            if (camera.name == "Main Camera")
            {
                mainCamera = camera;
            }
            else if (camera.name == "Second Camera")
            {
                secondCamera = camera;
            }
        }
        activeMainUICamera = mainCamera;
    }


    void Update()
    {
        //_camera = Camera.main;
        
        // Check if the target is null and hasn't been found yet
        if (target == null && !hasFoundTarget)
        {
            // Attempt to find the "PointCloud" GameObject by name
            GameObject pointCloudObject = GameObject.Find(followingObject);

            if (pointCloudObject != null)
            {
                target = pointCloudObject.transform;
                hasFoundTarget = true; // Set the flag to true once the target is found
            }
        }
        if (target != null)
        {
            mainCamera.transform.position = target.position + mainCamOffset;
            secondCamera.transform.position = target.position + secondCamOffset;
        }
        
        if (inputDetected())
        {
            var rotationY = getJoystickInput(out var rotationQuat);
            // Cameras are rotated differently since Main Camera (Birdseye view) is a orthographic camera and the other camera is perspective
            if (_isMainViewActive)
            {
                Quaternion deltaT = Quaternion.AngleAxis(rotationY, Vector3.forward);
                activeMainUICamera.transform.rotation *= deltaT;
            }
            else
            {
                activeMainUICamera.transform.rotation *= rotationQuat;
            }
                rotateCamera();
        }
        
    }
    
    public void SwapCameraTags(bool isMainViewActive)
    {
        if (isMainViewActive)
        {
            activeMainUICamera = mainCamera;
            _isMainViewActive = true;
        }
        else
        {
            _isMainViewActive = false;
            activeMainUICamera = secondCamera;
        }
    }
    
    
    private void rotateCamera()
    {
        Vector3 currentRotation = activeMainUICamera.transform.eulerAngles;
        // Set the z euler angle of transformation to 0
        currentRotation.z = 0; // Lock the Z-axis rotation
        activeMainUICamera.transform.eulerAngles = currentRotation;
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
        // Main Camera is orthographic, so it has no FOV. Therefore change the height (y) of the camera
        if (_isMainViewActive)
        {
            float numberDouble = number < 0 ? -0.5f : 0.5f;
            mainCamOffset.y += numberDouble;
        }
        else
        {
            secondCamera.fieldOfView += number;
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
