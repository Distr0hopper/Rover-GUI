using System;
using System.Collections;
using System.Collections.Generic;
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
        private VisualElement driveStopPanel;
        private VisualElement autoDrivePanel;
        private VisualElement manualDrivePanel;
        private VisualElement UWBPanel;
        private VisualElement GeoSAMAPanel;
        private VisualElement connectionState;
        private PopupWindow popupWindow;
        
        private Button commandButton;
        private Button stopButton;
        private Button forwardButton;
        private Button backwardButton;
        private Button leftButton;
        private Button rightButton;
        private Button incrementButton;
        private Button decrementButton;
        private Button autoDriveModeButton;
        private Button manualDriveModeButton;
        private Button missionModeUWBButton;
        private Button missionModeGeoSAMAButton;
        private Button switchViewButton;
        private Button hideModel;
        private Button launchButton;
        private Button trigger1Button;
        private Button trigger2Button;
        private Button trigger3Button;
        private Button trigger4Button;
        private Button activeTriggerButton;
        private Button startScanButton;
        
        private Label durationLabel;
        private Label distanceLabel;
        private Label arrivalLabel;

        private EnumField changeRobotDropdown;
        
        private ProgressBar scanProgressBar;

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
        private Color disabledButtonColor = new Color(0.26f, 0.26f, 0.26f,1f);
        private Color directionButtonColor = new Color(0.7f, 0.7f, 0.7f);
        
        #endregion

        #region Events

        // Event is triggered when Button is Clicked
        public static event Action OnStartDriving;
        public static event Action OnManualSteering;

        #endregion

        void Start()
        {
            // Get the elements from the UI
            var root = UIDocument.rootVisualElement;
            mainView = root.Q<VisualElement>("MainView");
            commandButton = root.Q<Button>("bStartDrive");
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
            missionModeUWBButton = root.Q<Button>("MissionModeUWB");
            missionModeGeoSAMAButton = root.Q<Button>("MissionModeGeoSAMA");
            manualDrivePanel = root.Q<VisualElement>("ManualDrivePanel");
            autoDrivePanel = root.Q<VisualElement>("AutoDrivePanel");
            UWBPanel = root.Q<VisualElement>("UWBPanel");
            GeoSAMAPanel = root.Q<VisualElement>("GeoSAMAPanel");
            driveStopPanel = root.Q<VisualElement>("DriveStopPanel");
            switchViewButton = root.Q<Button>("switchView");
            changeRobotDropdown = root.Q<EnumField>("RobotChoice");
            connectionState = root.Q<VisualElement>("ConnectionState");
            distanceLabel = root.Q<Label>("DistanceLabel");
            arrivalLabel = root.Q<Label>("ArrivalLabel");  
            hideModel = root.Q<Button>("HideModel");
            launchButton = root.Q<Button>("LaunchButton");
            trigger1Button = root.Q<Button>("Trigger1");
            trigger2Button = root.Q<Button>("Trigger2");
            trigger3Button = root.Q<Button>("Trigger3");
            trigger4Button = root.Q<Button>("Trigger4");
            startScanButton = root.Q<Button>("StartScan");
            scanProgressBar = root.Q<ProgressBar>("ScanProgress");
            popupWindow = root.Q<PopupWindow>("PopupWindow");
            
            
            
            
            // Click on mainview, screenpoint is converted to worldpoint
            mainView.RegisterCallback<ClickEvent>(ScreenToWorld);

            // Clicked methods for button //
            commandButton.clicked += () => { SendStearingCommand(); };
            commandButton.SetEnabled(false); //Cannot be clicked at beginning, need to set goal first 
            
            trigger1Button.clicked += () => { SetActiveTriggerButton(trigger1Button); };
            trigger2Button.clicked += () => { SetActiveTriggerButton(trigger2Button); };
            trigger3Button.clicked += () => { SetActiveTriggerButton(trigger3Button); };
            trigger4Button.clicked += () => { SetActiveTriggerButton(trigger4Button); };
            
            launchButton.clicked += () => { LaunchActiveUWB(); };
            launchButton.SetEnabled(false); //Cannot be clicked at beginning, need to select UWB Sensor first
            
            startScanButton.clicked += () => { StartScan(); };
            
            popupWindow.confirmed += () => { EnableLaunch(); };
            popupWindow.canceled += () => { DisableLaunch(); };
            
            stopButton.clicked += () =>
            {
                SetStearingInformation(stopButton);
                ManualSteeringButtonClicked();
                FlashStopButton();
                ChangeMarkerColor(false);
                ChangeStartDrivingButton(true);
            };
            forwardButton.clicked += () =>
            {
                ChangeManualSteeringButtonColor(forwardButton);
                SetStearingInformation(forwardButton);
                ChangeStartDrivingButton(true);
            };
            backwardButton.clicked += () =>
            {
                ChangeManualSteeringButtonColor(backwardButton);
                SetStearingInformation(backwardButton);
                ChangeStartDrivingButton(true);
            };
            leftButton.clicked += () =>
            {
                ChangeManualSteeringButtonColor(leftButton);
                SetStearingInformation(leftButton);
                ChangeStartDrivingButton(true);
            };
            rightButton.clicked += () =>
            {
                ChangeManualSteeringButtonColor(rightButton);
                SetStearingInformation(rightButton);
                ChangeStartDrivingButton(true);
            };
            
            
            autoDriveModeButton.clicked += () => { SetOperationMode(Robot.OperationMode.autoDrive);  ChangeStartDrivingButton(false);};
            manualDriveModeButton.clicked += () => { SetOperationMode(Robot.OperationMode.manualDrive);  ChangeStartDrivingButton(false); ChangeManualSteeringButtonColor(null, true); };
            missionModeUWBButton.clicked += () => { SetOperationMode(Robot.OperationMode.uwbMission); };
            missionModeGeoSAMAButton.clicked += () => { SetOperationMode(Robot.OperationMode.geoSamaMission); };
            incrementButton.clicked += () => { IncrementDuration(); };
            decrementButton.clicked += () => { DecrementDuration(); };
            
            switchViewButton.clicked += () => { SwitchView(); };
            
            hideModel.clicked += () => { toggleModelVisability() ;};
            
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
            if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
            {
                missionModeUWBButton.style.display = DisplayStyle.None;
                missionModeGeoSAMAButton.style.display = DisplayStyle.None;
            }
            else
            {
                missionModeUWBButton.style.display = DisplayStyle.Flex;
                missionModeGeoSAMAButton.style.display = DisplayStyle.Flex;
            }
            //Reset the Operation Mode and show the auto drive panel
            Robot.Instance._operationMode = Robot.OperationMode.autoDrive;
            SetOperationMode(Robot.Instance._operationMode);
            
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
        
        private void toggleModelVisability()
        {
            if (Robot.Instance.IsModelActive())
            {
                hideModel.text = "Show 3D Model";
                Robot.Instance.HideModel();
            }
            else
            {
                hideModel.text = "Hide 3D Model";
                Robot.Instance.ShowModel();
            }
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
            Robot.ACTIVEROBOT selectedRobot = (Robot.ACTIVEROBOT) newValue;
            Robot.Instance.ActiveRobot = selectedRobot;
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
            ChangeStartDrivingButton(true);
            ChangeMarkerColor(false);
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
        private void SendStearingCommand()
        {
            if (Robot.Instance._operationMode == Robot.OperationMode.autoDrive)
            {
            OnStartDriving?.Invoke();
            ChangeStartDrivingButton(false);
            ChangeMarkerColor(true);
            } else if (Robot.Instance._operationMode == Robot.OperationMode.manualDrive)
            {
                ManualSteeringButtonClicked();
            }
        }
        
        /*
         * Change the state and the color of the Start Driving button to grey, so it can't be clicked again until a new goal is set
         */
        public void ChangeStartDrivingButton(bool setActive)
        {
            if (setActive)
            { 
                commandButton.SetEnabled(true);
                commandButton.style.backgroundColor = new StyleColor(blueButtonColor);
            }
            else
            {
            commandButton.style.backgroundColor = new StyleColor(Color.grey);
            //Make the button unclickable
            commandButton.SetEnabled(false);
            }
        }
        
        /*
         * Event listener, waiting for a Coroutine to be started to flash the stop button
         */
        public void FlashStopButton()
        {
            StartCoroutine(FlashButtonCoroutine(stopButton, 3, Color.yellow, 0.5f));
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
         * Waiting for any manual steering button to be clicked (in ROSSender class)
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
        private void SetOperationMode(Robot.OperationMode mode)
        {
            if (mode == Robot.OperationMode.autoDrive)
            {
                Robot.Instance._operationMode = Robot.OperationMode.autoDrive;
                SetAutoDriveMode();
            }
            else if (mode == Robot.OperationMode.manualDrive)
            {
                Robot.Instance._operationMode = Robot.OperationMode.manualDrive;
                SetManualDriveMode();
            } 
            else if (mode == Robot.OperationMode.uwbMission)
            {
                Robot.Instance._operationMode = Robot.OperationMode.uwbMission;
                SetUWBMode();
            }
            else if (mode == Robot.OperationMode.geoSamaMission)
            {
                Robot.Instance._operationMode = Robot.OperationMode.geoSamaMission;
                SetGeoSAMAMode();
            }
            ChangeOperationButtonColor();
        }
            
        /*
         * Set the panel display depending on the operation mode.
         * The drive-stop panel is displayed in manual and auto drive mode
         */
        private void SetPanelDisplay(Robot.OperationMode mode)
        {
            manualDrivePanel.style.display = (mode == Robot.OperationMode.manualDrive) ? DisplayStyle.Flex : DisplayStyle.None;
            autoDrivePanel.style.display = (mode == Robot.OperationMode.autoDrive) ? DisplayStyle.Flex : DisplayStyle.None;
            UWBPanel.style.display = (mode == Robot.OperationMode.uwbMission) ? DisplayStyle.Flex : DisplayStyle.None;
            GeoSAMAPanel.style.display = (mode == Robot.OperationMode.geoSamaMission) ? DisplayStyle.Flex : DisplayStyle.None;
            driveStopPanel.style.display = (mode == Robot.OperationMode.manualDrive || mode == Robot.OperationMode.autoDrive) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetManualDriveMode()
        {
            SetPanelDisplay(Robot.OperationMode.manualDrive);
        }

        private void SetAutoDriveMode()
        {
            SetPanelDisplay(Robot.OperationMode.autoDrive);
        }

        private void SetUWBMode()
        {
            SetPanelDisplay(Robot.OperationMode.uwbMission);
        }

        private void SetGeoSAMAMode()
        {
            SetPanelDisplay(Robot.OperationMode.geoSamaMission);
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
        
        
        /*
         * Method that checks which Trigger Button was clicked and sets active flag to true so its border color changes to green 
         */
        public void SetActiveTriggerButton(Button clickedButton)
        {
            if (activeTriggerButton == clickedButton)
            {
                // Reset the button state and clear the last clicked button
                ResetButtonState(clickedButton);
                activeTriggerButton = null;
                launchButton.SetEnabled(false);
                Robot.Instance.UwbTrigger = Robot.UWBTRIGGER.noTrigger;
            } else
            {
                // Reset the state of the previously clicked button
                if (activeTriggerButton != null)
                {
                    ResetButtonState(activeTriggerButton);
                }

                // Change the border color of the clicked button
                clickedButton.style.borderBottomColor = Color.green;
                clickedButton.style.borderTopColor = Color.green;
                clickedButton.style.borderLeftColor = Color.green;
                clickedButton.style.borderRightColor = Color.green;

                // Update the last clicked button
                activeTriggerButton = clickedButton;
                
                // Set the active UWB in the robot model
                SetActiveUWB();
                
                if (activeTriggerButton.style.backgroundColor == disabledButtonColor)
                {
                    //launchButton.text = "UWB already launched, try again?";
                    //launchButton.style.backgroundColor = new StyleColor(Color.yellow);
                    popupWindow.style.display = DisplayStyle.Flex;
                }
                else
                {
                    // Enable the launch button
                    launchButton.SetEnabled(true);
                }
            }
        }

        /*
         * Set the active UWB in the robot model
         */
        public void SetActiveUWB()
        {
            if (activeTriggerButton.name == "Trigger1")
            {
               Robot.Instance.UwbTrigger = Robot.UWBTRIGGER.trigger1;
            }
            else if (activeTriggerButton.name == "Trigger2")
            {
                Robot.Instance.UwbTrigger = Robot.UWBTRIGGER.trigger2;
            }
            else if (activeTriggerButton.name == "Trigger3")
            {
                Robot.Instance.UwbTrigger = Robot.UWBTRIGGER.trigger3;
            }
            else if (activeTriggerButton.name == "Trigger4")
            {
                Robot.Instance.UwbTrigger = Robot.UWBTRIGGER.trigger4;
            }
        }
        
        /*
         * Launch the active UWB and disable the launch button
         */
        public void LaunchActiveUWB()
        {
            //TODO: Send message to ROS to launch the UWB
            //ROSSender.LaunchUWB();
            // Set the Background color of the launched Button to grey and disable it
            activeTriggerButton.style.backgroundColor = new StyleColor(disabledButtonColor);
            //activeTriggerButton.SetEnabled(false);
            Debug.Log("Launching: " + Robot.Instance.UwbTrigger);
            launchButton.SetEnabled(false);
        }

        /*
         * Start the coroutine to fill the progress bar
         */
        public void StartScan()
        {
            // TODO: Send message to ROS to start the scan 
            StartCoroutine(FillProgressBar());
        }
        
        
        /*
         * If the popup is confirmed, enable the launch button again
         */
        private void EnableLaunch()
        {
            launchButton.SetEnabled(true);
            popupWindow.style.display = DisplayStyle.None;
        }
        
        /*
         * If the popup is canceled, disable the launch button
         */
        private void DisableLaunch()
        {
            launchButton.SetEnabled(false);
            popupWindow.style.display = DisplayStyle.None;
        }

        #region USS Styling Methods

        /*
         * Active Manual Stearing Button in Blue and the others in Grey.
         * When reset is true, all buttons are set to grey.
         */
        public void ChangeManualSteeringButtonColor(Button clickedButton, bool reset = false)
        {
            Button[] buttons = { forwardButton, backwardButton, leftButton, rightButton };
            if (reset)
            {
                foreach (Button button in buttons)
                {
                    button.style.backgroundColor = new StyleColor(directionButtonColor);
                }
            }
            else
            {
                foreach (Button button in buttons)
                {
                    button.style.backgroundColor = (button == clickedButton) ? new StyleColor(blueButtonColor) : new StyleColor(directionButtonColor);
                }
            }


        }
        
        /*
         * Change the Color of the Modes at the top of the GUI. The active mode is green, the others are blue.
         */
        private void ChangeOperationButtonColor()
        {
            Robot.OperationMode mode = Robot.Instance._operationMode;

            // Dictionary to map each mode to its corresponding active button
            Dictionary<Robot.OperationMode, Button> modeToButton = new Dictionary<Robot.OperationMode, Button>
            {
                { Robot.OperationMode.autoDrive, autoDriveModeButton },
                { Robot.OperationMode.manualDrive, manualDriveModeButton },
                { Robot.OperationMode.uwbMission, missionModeUWBButton },
                { Robot.OperationMode.geoSamaMission, missionModeGeoSAMAButton }
            };

            // Iterate through each mode-button pair
            foreach (var pair in modeToButton)
            {
                pair.Value.style.backgroundColor = new StyleColor(pair.Key == mode ? greenButtonColor : blueButtonColor);
            }
        }
        
        /*
         * Coroutine to fill the progress bar
         */
        private IEnumerator FillProgressBar()
        {
            scanProgressBar.value = 0;
            scanProgressBar.title = "Scanning...";
            float duration = 5f;
            float timePassed = 0f;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                scanProgressBar.value = (timePassed / duration) * scanProgressBar.highValue;
                yield return null;
            }
            scanProgressBar.value = scanProgressBar.highValue;
            // Set text to "Scan finished" from the progress bar
            scanProgressBar.title = "Scan finished";
        }
        
        /*
         * Change the color of the marker depending on if it is a new goal or not
         */
        private void ChangeMarkerColor(bool setActive)
        {
            if (setActive)
            {
                arrow.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                arrow.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }

        #endregion
       
        #region Helper Methods
        
        /*
         * Reset Trigger Button State
         */
        private void ResetButtonState(Button button)
        {
            // Reset the border color of the button
            button.style.borderBottomColor = Color.clear;
            button.style.borderTopColor = Color.clear;
            button.style.borderLeftColor = Color.clear;
            button.style.borderRightColor = Color.clear;
        }
        
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

