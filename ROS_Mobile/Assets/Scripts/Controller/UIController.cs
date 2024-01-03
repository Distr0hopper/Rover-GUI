using System;
using System.Collections;
using System.Linq;
using Model;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
namespace myUIController
{

    public class UIController : MonoBehaviour
    {
        #region UIElements
        private VisualElement mainView; //View which shows RenderTexture from Camera 
        private VisualElement secondView; //View which shows Top-Down view of the map
        private VisualElement manualDrivePanel;
        private VisualElement autoDrivePanel;
        private VisualElement connectionState;
        
        private Button driveButton;
        private Button stopButton;
        private Button forwardButton;
        private Button backwardButton;
        private Button leftButton;
        private Button rightButton;
        private Button incrementButton;
        private Button decrementButton;
        private Button autoDriveModeButton;
        private Button manualDriveModeButton;
        private Button switchViewButton;
        
        private Label durationLabel;
        private Label distanceLabel;
        private Label arrivalLabel;

        private EnumField changeRobotDropdown;

        #endregion

        #region Cameras
        
        private Camera mainCamera;
        private Camera secondCamera;
        
        #endregion
        
        #region Public Properties

        [SerializeField] public GameObject marker;
        [SerializeField] public RenderTexture mainViewTexture;
        [SerializeField] public RenderTexture secondViewTexture;
        public static bool isMainActive = true;
        [HideInInspector] public UIDocument UIDocument { private get;  set; }
        public CameraController cameraController { private get;  set; }
        public ConnectionController connectionController { private get; set; }
        [SerializeField] public GameObject arrow;
        
        
        #endregion

        #region Private Properties
        
        private Vector3 clickPosition;
        private Color blueButtonColor = new Color(0.0f, 0.121f, 1f);
        private Color greenButtonColor = new Color(0.0f, 0.7119f, 0.1031f);
        
        #endregion

        #region Events

        // Event is triggered when Button is Clicked
        public static event Action OnStartDriving;
        public static event Action OnManualSteering;

        #endregion

        #region ENUMS

        private enum OperationMode
        {
            autoDrive,
            manualDrive
        }

        private OperationMode _operationMode = OperationMode.autoDrive;

        #endregion

        void Start()
        {
            // Get the elements from the UI
            var root = UIDocument.rootVisualElement;
            mainView = root.Q<VisualElement>("MainView");
            driveButton = root.Q<Button>("bStartDrive");
            stopButton = root.Q<Button>("stopButton");
            secondView = root.Q<VisualElement>("secondView");
            forwardButton = root.Q<Button>("fwdButton");
            backwardButton = root.Q<Button>("bwdButton");
            leftButton = root.Q<Button>("lftButton");
            rightButton = root.Q<Button>("rgtButton");
            incrementButton = root.Q<Button>("incButton");
            decrementButton = root.Q<Button>("decButton");
            durationLabel = root.Q<Label>("Time");
            autoDriveModeButton = root.Q<Button>("AutoDriveMode");
            manualDriveModeButton = root.Q<Button>("ManualDriveMode");
            manualDrivePanel = root.Q<VisualElement>("ManualDrivePanel");
            autoDrivePanel = root.Q<VisualElement>("AutoDrivePanel");
            switchViewButton = root.Q<Button>("switchView");
            changeRobotDropdown = root.Q<EnumField>("RobotChoice");
            connectionState = root.Q<VisualElement>("ConnectionState");
            distanceLabel = root.Q<Label>("DistanceLabel");
            arrivalLabel = root.Q<Label>("ArrivalLabel");  

            
            
            // Click on mainview, screenpoint is converted to worldpoint
            mainView.RegisterCallback<ClickEvent>(ScreenToWorld);

            // Clicked methods for button //
            driveButton.clicked += () => { StartDrivingButtonClicked(); };
            driveButton.SetEnabled(false); //Cannot be clicked at beginning, need to set goal first 
            
            stopButton.clicked += () =>
            {
                SetStearingInformation(stopButton);
                ManualSteeringButtonClicked();
                FlashStopButton();
            };
            forwardButton.clicked += () =>
            {
                SetStearingInformation(forwardButton);
                ManualSteeringButtonClicked();
            };
            backwardButton.clicked += () =>
            {
                SetStearingInformation(backwardButton);
                ManualSteeringButtonClicked();
            };
            leftButton.clicked += () =>
            {
                SetStearingInformation(leftButton);
                ManualSteeringButtonClicked();
            };
            rightButton.clicked += () =>
            {
                SetStearingInformation(rightButton);
                ManualSteeringButtonClicked();
            };
            
            
            autoDriveModeButton.clicked += () => { SetOperationMode(OperationMode.autoDrive); };
            manualDriveModeButton.clicked += () => { SetOperationMode(OperationMode.manualDrive); };
            incrementButton.clicked += () => { IncrementDuration(); };
            decrementButton.clicked += () => { DecrementDuration(); };
            
            switchViewButton.clicked += () => { SwitchView(); };
            
            //Method when Dropdown Menue is clicked
            changeRobotDropdown.RegisterValueChangedCallback(evt => {ChangeActiveRobotDropdown(evt.newValue); });

            connectionController.OnConnectionStatusChanged += HandleConnectionStatusChanged;
            StartCoroutine(InitializeAfterLayout());
        }
        
        /*
         * Method to reset the UI to the default state, needed when active robot switched
         */
        public void ResetUI()
        {
            //Reset the Operation Mode and show the auto drive panel
            _operationMode = OperationMode.autoDrive;
            SetOperationMode(_operationMode);
            
            //Reset the Marker to the position of the robot
            marker.transform.position = new Vector3(Robot.Instance.CurrentPos.x, 0.75f, Robot.Instance.CurrentPos.z);
            
            //Reset the Distance Label
            distanceLabel.text = "0 m";
            
            //Reset the Arrival Label
            arrivalLabel.text = "0 m";
            
            //Reset the Active View to the Main View
            if (!isMainActive)
                SwitchView();
            
            //Reset the Duration 
            durationLabel.text = "5";
            Robot.Instance.Duration = 5;
            
            //Reset the FOV
            cameraController.ResetCameraFOV();
            
            //Reset the Camera Rotation
            cameraController.ResetCameraRotation();
        }
        
        /*
         * When closing the application and the Views are still changed,
         * set the RenderTextures to the correct start values.
         * This is because the main view has the resolution of 1912x1638 and the second view has the resolution of 815x737
         */
        private void OnApplicationQuit()
        {
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
            // Unsubscribe when the application is closed
            connectionController.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
        }
        
        /*
         * Change the color of the connection state in the UI depending on the connection status
         */
        private void HandleConnectionStatusChanged(bool isConnected)
        {
            // Update the connection state in the UI
            connectionState.style.backgroundColor = isConnected ? new StyleColor(Color.green) : new StyleColor(Color.red);
        }
        
        /*
         * Change the active robot depending on the selected value in the dropdown menu
         */
        private void ChangeActiveRobotDropdown(Enum newValue)
        {
            BasicController.ACTIVEROBOT selectedRobot = (BasicController.ACTIVEROBOT) newValue;
            BasicController.ActiveRobot = selectedRobot;
            connectionController.ChangeRobotIP();
            ResetUI();
        }
        
        /*
         * Wait until the layout is done and then resize the RenderTextures to the correct size
         */
        private IEnumerator InitializeAfterLayout()
        {
            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
        }


        /*
         * Code adapted from:
         * https://forum.unity.com/threads/solved-how-to-get-world-point-coordinates-from-a-render-texture.539393/
         * Only shoot the ray if the click was on the main view, otherwise the ray would be shot even if clicked on the buttons
         * which are on the main view.
         */
        private void ScreenToWorld(ClickEvent evt)
        {
            // Return if not clicked on mainView (clicked on button then)
            if(evt.target != mainView) return;
            clickPosition = evt.localPosition;
            // Convert click position to a proportion of the VisualElement's size (because it is in Pixels and not in World Units)
            clickPosition.x /= mainView.resolvedStyle.width;
            clickPosition.y /= mainView.resolvedStyle.height;
            
            Vector3 viewportPoint = new Vector3(clickPosition.x, 1 - clickPosition.y); //Invert Y, because (0.0) is bottom left in UI, but top left in camera
            Vector3 worldPosition = new Vector3();
            Ray ray = cameraController.activeMainUICamera.ViewportPointToRay(viewportPoint);

            //Raycast it against ground Plane, shorthand for a vector projection.
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                worldPosition = ray.GetPoint(enter);
                marker.transform.position = new Vector3(worldPosition.x, 0.75f, worldPosition.z); //Add 0.75f to make above the robot model

                // Set the world coordinates in the robot model
                Robot.Instance.SetGoalInWorldPos(worldPosition);
            }
            CalculateDistance(worldPosition);
            ChangeStartDrivingButtonColor(true);
            ChangeMarkerColor(true);
            
        }
        
        /*
         * Calculate the distance between the robot and the goal
         */
        private void CalculateDistance(Vector3 worldPosition)
        {
            // Calculate the distance between the robot and the goal
            float distance = Vector3.Distance(Robot.Instance.CurrentPos, worldPosition);
            Debug.Log("Distance: " + distance + " m");
            UpdateDistanceLabel(distance);
        }
        
        private void UpdateDistanceLabel(float distance)
        {
            // The distance only in 2 decimal places
            distance = (float) Math.Round(distance, 2);
            string distanceString = distance.ToString();
            // Add " m" to the distance string
            distanceString += " m";
            distanceLabel.text = distanceString;
        }
        
        /*
         * Event listener:
         * Waiting for button click event to start driving to the goal
         */
        private void StartDrivingButtonClicked()
        {
            OnStartDriving?.Invoke();
            ChangeStartDrivingButtonColor(false);
            ChangeMarkerColor(false);
        }
        
        /*
         * Change the color of the Start Driving button to grey, so it can't be clicked again until a new goal is set
         */
        public void ChangeStartDrivingButtonColor(bool newGoal)
        {
            if (newGoal)
            { 
                driveButton.SetEnabled(true);
                driveButton.style.backgroundColor = new StyleColor(blueButtonColor);
            }
            else
            {
            driveButton.style.backgroundColor = new StyleColor(Color.grey);
            //Make the button unclickable
            driveButton.SetEnabled(false);
            }
        }
        
        /*
         * Change the color of the marker depending on if it is a new goal or not
         */
        private void ChangeMarkerColor(bool newGoal)
        {
            if (newGoal)
            {
            arrow.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
            arrow.GetComponent<MeshRenderer>().material.color = Color.green;
            }
        }
        
        /*
         * Event listener, waiting for a Coroutine to be started to flash the stop button
         */
        public void FlashStopButton()
        {
            StartCoroutine(FlashButtonCoroutine(stopButton, 3, Color.black, 0.5f));
        }
        
        /*
         * Coroutine to flash the stop button. It flashes 3 times in black with a duration of 0.5 seconds
         */
        private IEnumerator FlashButtonCoroutine(Button button, int flashes, Color flashColor, float duration)
        {
            Color originalColor = button.resolvedStyle.backgroundColor; // Store original color
            for (int i = 0; i < flashes; i++)
            {
                // Change to flash color
                button.style.backgroundColor = flashColor;
                yield return new WaitForSeconds(duration / 2);

                // Revert to original color
                button.style.backgroundColor = originalColor;
                yield return new WaitForSeconds(duration / 2);
            }
        }

        /*
         * Event listener:
         * Waiting for any manual steering button to be clicked
         */
        private void ManualSteeringButtonClicked()
        {
            OnManualSteering?.Invoke();
        }

        /*
         * Set the steering information in the robot model depending on the clicked button
         */
        private void SetStearingInformation(Button clickedButton)
        {
            if (clickedButton == forwardButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.forward;
            }
            else if (clickedButton == backwardButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.backward;
            }
            else if (clickedButton == leftButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.left;
            }
            else if (clickedButton == rightButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.right;
            }
            else if (clickedButton == stopButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.stop;
            }
        }

        /*
         * Increment the duration of the robot model and update the duration label in the UI
         */
        private void IncrementDuration()
        {
            Robot.Instance.IncrementSpeed();
            UpdateDurationLabelInView();
        }
        
        /*
         * Decrement the duration of the robot model and update the duration label in the UI
         */
        private void DecrementDuration()
        {
            Robot.Instance.DecrementSpeed();
          UpdateDurationLabelInView();
        }
        
        /*
         * Update the duration label in the UI with the duration of the robot model
         */
        private void UpdateDurationLabelInView()
        {
            durationLabel.text = Robot.Instance.Duration.ToString(); 
        }
        
        /*
         * Change the Operation Mode 
         */
        private void SetOperationMode(OperationMode mode)
        {
            if (mode == OperationMode.autoDrive)
            {
                _operationMode = OperationMode.autoDrive;
                SetAutoDriveMode();
            }
            else if (mode == OperationMode.manualDrive)
            {
                _operationMode = OperationMode.manualDrive;
                SetManualDriveMode();
            }
            ChangeOperationButtonColor();
        }
            
        /*
         * Show the manual drive panel when clicked on the manual drive mode button
         */
        private void SetManualDriveMode()
        {
            manualDrivePanel.style.display = DisplayStyle.Flex;
            autoDrivePanel.style.display = DisplayStyle.None;
        }
        
        /*
         * Show the auto drive panel when clicked on the auto drive mode button (manual drive panel is hidden on auto drive mode)
         */
        private void SetAutoDriveMode()
        {
            autoDrivePanel.style.display = DisplayStyle.Flex;
            manualDrivePanel.style.display = DisplayStyle.None;
        }
        
        /*
         * Change the background color of the active mode
         */
        
        private void ChangeOperationButtonColor()
        {
            if (_operationMode == OperationMode.autoDrive)
            {
                autoDriveModeButton.style.backgroundColor = new StyleColor(greenButtonColor);
                manualDriveModeButton.style.backgroundColor = new StyleColor(blueButtonColor);
            }
            else if (_operationMode == OperationMode.manualDrive)
            {
                manualDriveModeButton.style.backgroundColor = new StyleColor(greenButtonColor);
                autoDriveModeButton.style.backgroundColor = new StyleColor(blueButtonColor);
            }
        }
        
        /*
         * Switch the active view (main view and second view)
         */
        private void SwitchView()
        {
            // Toggle the view state
            isMainActive = !isMainActive;

            // Swap textures references
            SwapTextures();

            // Resize textures based on the new active view
            ResizeTextures();

            // Update background images
            UpdateBackgroundImages();

            // Swap active camera which renders the onto the mainView
            cameraController.SwapCamera();
        }

        #region Helper Methods
        /*
         * Swap the main and second view textures
         */
        private void SwapTextures()
        {
            (mainViewTexture, secondViewTexture) = (secondViewTexture, mainViewTexture);
            //The same as the following, just to show what a big programmer cock i have 
            //RenderTexture temp = mainViewTexture;
            //mainViewTexture = secondViewTexture;
            //secondViewTexture = temp;
        }
        
        /*
         * Resize the textures so it fits the VisualElement which they are rendered on
         */
        private void ResizeTextures()
        {
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
        }
        
        /*
         * Set the rendered textures to the VisualElement Background
         */
        private void UpdateBackgroundImages()
        {
            mainView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(mainViewTexture));
            secondView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(secondViewTexture));
        }
        
        #endregion
        
    }
    
}

