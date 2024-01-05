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
    private Button resetButton;
    
    #endregion

    #region Private Fields

    private Vector2 m_JoystickPointerDownPosition;
    private Vector2 m_JoystickDelta; // Between -1 and 1
    private Camera mainCamera;
    private Camera secondCamera;

    // Initial values for the cameras (used for reset)
    private float secondCamStartFOV = 90f;
    private float mainCamStartSize = 5f;
    private Vector3 secondCamStartRot = new Vector3(0.97f, 0f, 0f);
    private Vector3 mainCamStartRot = new Vector3(90f, 0f, 0f);
    
    #endregion

    #region Public Fields

    public Camera activeMainUICamera { get; private set; }
    public float rotationSpeed = 1.25f;
    [HideInInspector] public UIDocument UIDocument { private get; set; }

    #endregion

    #region Serialized Fields

    [SerializeField] private Vector3 mainCamOffset;
    [SerializeField] private Vector3 secondCamOffset;

    #endregion

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
        resetButton = root.Q<Button>("ResetButton");
        m_decreaseFOV.clicked += () => { ChangeFOV(5); ShowResetButton();}; // If you press FOV + you want to "zoom in"
        m_inreaseFOV.clicked += () => { ChangeFOV(-5); ShowResetButton();}; // If you press FOV - you want to "zoom out"
        resetButton.clicked += () => { ResetCameraRotation(); ResetCameraFOV(); HideResetButton(); UIController.UpdateZoomDistanceLabel(0,true);};
    }

    /*
     * Get The camera objects from the scene at start up
     */
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
        UpdateCameraPosition();

        if (InputDetected())
        {
            ShowResetButton(); //Show the reset button if the joystick is moved
            var rotationY = GetJoystickInput(out var rotationQuat);
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
                RotateCamera();
        }
    }

    /*
     * Make the cameras follow the position of the robot.
     * Adds a offset to the position of the camera so that the camera is not directly inside the robot
     */
    private void UpdateCameraPosition()
    {
        mainCamera.transform.position = Robot.Instance.CurrentPos + mainCamOffset;
        secondCamera.transform.position = Robot.Instance.CurrentPos + secondCamOffset;
    }

    /*
     * Swap the active camera between the main camera and the second camera
     */
    public void SwapCamera()
    {
        if (UIController.isMainActive)
        {
            activeMainUICamera = mainCamera;
        }
        else
        {
            activeMainUICamera = secondCamera;
        }
    }
    
    /*
     * Rotate the camera
     */
    private void RotateCamera()
    {
        Vector3 currentRotation = activeMainUICamera.transform.eulerAngles;
        // Set the z euler angle of transformation to 0
        currentRotation.z = 0; // Lock the Z-axis rotation
        activeMainUICamera.transform.eulerAngles = currentRotation;
    }
    
    /*
     * Reset the camera FOV to its initial value
     */
    public void ResetCameraFOV()
    {
        mainCamera.orthographicSize = mainCamStartSize;
        secondCamera.fieldOfView = secondCamStartFOV;
    }
    
    /*
     * Reset the camera rotation to its initial value
     */
    public void ResetCameraRotation()
    {
        mainCamera.transform.rotation = Quaternion.Euler(mainCamStartRot);
        secondCamera.transform.rotation = Quaternion.Euler(secondCamStartRot);
    }
    
    /*
     * Get the input from the joystick and return the rotation value
     */
    private float GetJoystickInput(out Quaternion rotationQuat)
    {
        float rotationX = -m_JoystickDelta.y * rotationSpeed;
        float rotationY = m_JoystickDelta.x * rotationSpeed;
        float rotationZ = 0;

        rotationQuat = Quaternion.Euler(rotationX, rotationY, rotationZ);
        return rotationY;
    }
    
    /*
     * Check if the joystick is moved. Expensive Method, since its always polling.
     * TODO: Change to event based
     */
    private bool InputDetected()
    {
        return m_JoystickDelta != Vector2.zero;
    }

    /*
     * Change the FOV of the camera
     */
    void ChangeFOV(int number)
    {
        // If the MainView is active, the Birdseye Camera is rendered to it 
        if (UIController.isMainActive)
        {
            // Step size of 5 is to much, so use .5 steps instead in the orthographic camera (divide by 10)
            float numbAsFloat = number / 10f; 
            // Change the height of the camera
            //mainCamOffset.y += numbAsFloat;
            if (mainCamera.orthographicSize <= 1) return;
            mainCamera.orthographicSize += numbAsFloat;
            UIController.UpdateZoomDistanceLabel(number / 10f);
        }
        else
        {
            if (secondCamera.fieldOfView <= 0) return;
            secondCamera.fieldOfView += number;
        }
        
    }
    
    /*
     * Show the Reset button only if changed FOV or rotated camera
     */
    private void ShowResetButton()
    {
        resetButton.style.display = DisplayStyle.Flex;
    }
    
    /*
     * Hide the Reset button if clicked on it
     */
    private void HideResetButton()
    {
        resetButton.style.display = DisplayStyle.None;
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
