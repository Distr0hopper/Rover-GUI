using myUIController;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Model;

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
    
    [SerializeField] private Vector3 mainCamOffset;
    [SerializeField] private Vector3 secondCamOffset;
    [HideInInspector] public UIDocument UIDocument { private get; set; }
    
  
    public Camera activeMainUICamera { get; private set; }
    private Camera mainCamera;
    private Camera secondCamera;
    
    public Robot robot { private get; set; }
    
    [FormerlySerializedAs("worldObjectPushStrength")] public float rotationSpeed = 1.25f;

    void OnEnable()
    {
        var root = UIDocument.rootVisualElement;
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
        mainCamera.transform.position = robot.currentPos + mainCamOffset;
        secondCamera.transform.position = robot.currentPos + secondCamOffset;
        
        if (inputDetected())
        {
            var rotationY = getJoystickInput(out var rotationQuat);
            // Cameras are rotated differently since Main Camera (Birdseye view) is a orthographic camera and the other camera is perspective
            if (UIController.isMainActive)
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
    
    public void SwapCamera(bool isMainViewActive)
    {
        if (isMainViewActive)
        {
            activeMainUICamera = mainCamera;
        }
        else
        {
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
        if (UIController.isMainActive)
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
