using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils;

namespace myUIController
{
    public class UIController : MonoBehaviour
    {
        #region UIElements

        // Bottom Panel
        private Button closeButton;
        private Button debugButton;
        private Label errorLabel;
        private VisualElement errorMessagePanel;

        private VisualElement mainView; //View which shows RenderTexture from Camera 
        private VisualElement secondView; //View which shows Top-Down view of the map
        private VisualElement driveStopPanel;
        private VisualElement autoDrivePanel;
        private VisualElement manualDrivePanel;
        private VisualElement UWBPanel;
        private VisualElement GeoSAMAPanel;
        private VisualElement rieglPanel;
        private VisualElement cameraWindowCharlie;
        private VisualElement cameraWindowLars;
        private PopupWindow popupWindow;

        // Connection Panel
        private VisualElement connectionState;
        private Label connectionLabel;

        // Command-Stop Panel
        private Button commandButton;
        private Button stopButton;

        // Manual Drive Panel
        private Button forwardButton;
        private Button backwardButton;
        private Button turnCWButton;
        private Button turnCCWButton;
        private Button driveMode;
        private Button turnMode;
        private Button stepsizeMin;
        private Button stepsizeMid;
        private Button stepsizeMax;
        private Button incrementButton;
        private Button decrementButton;
        private Button resetStepsizeButton;

        // Top Panel for Modes
        private Button autoDriveModeButton;
        private Button manualDriveModeButton;
        private Button missionModeUWBButton;
        private Button missionModeGeoSAMAButton;
        private Button scanModeButton;

        // Main Panel 
        private Button switchViewButton;
        private Button hideModel;
        private Button hideCamera;

        //UWB Mode Panel
        private Button launchButton;
        private Button trigger1Button;
        private Button trigger2Button;
        private Button trigger3Button;
        private Button trigger4Button;
        private Button activeTriggerButton; //Button which is currently active (trigger 1-4)
        private Button rotate45Button;
        private Button rotate90Button;
        private Button calibrateButton;
        private Button confirmedUwbCalibDone;

        //GeoSAMA Mode Panel
        private Button startScanButton;

        //Drive Labels
        private Label valueLabel;
        private Label unitLabel;
        private Label inputTypeLabel;
        private Label distanceLabel;
        private Label arrivalLabel;

        //Riegl Panel
        private Button startRieglScanButton;
        private Label timePasedLabel;
        private VisualElement rieglCautionPanel;

        private EnumField changeRobotDropdown;

        private ProgressBar scanProgressBar;
        private ProgressBar scanProgressRiegl;

        private Button lastClickedButton;

        #endregion

        #region Public Properties

        [SerializeField] public GameObject marker;
        [SerializeField] public RenderTexture mainViewTexture;
        [SerializeField] public RenderTexture secondViewTexture;

        [FormerlySerializedAs("imageTexture")] [SerializeField]
        public RenderTexture imageTextureRealsense;

        [SerializeField] public RenderTexture imageTextureGeoSAMA;
        public static bool isMainActive = true;
        [HideInInspector] public UIDocument UIDocument { private get; set; }
        public CameraController cameraController { private get; set; }
        public ConnectionController connectionController { private get; set; }
        [SerializeField] public GameObject arrow;

        #endregion

        #region Private Properties

        private Vector3 clickPosition;
        private float distanceStepSize = 0.5f;
        private float turnStepSize = 5f;
        private Color blueButtonColor = new Color(0.0f, 0.121f, 1f);
        private Color greenButtonColor = new Color(0.0f, 0.7119f, 0.1031f);
        private Color disabledButtonColor = new Color(0.26f, 0.26f, 0.26f, 1f);
        private Color unselectedButtonColor = new Color(0.73f, 0.73f, 0.73f);
        float distance = 0;
        public bool stopButtonClicked = false;

        #endregion

        #region Events

        // Event is triggered when Button is Clicked
        public static event Action OnStartDriving;
        public static event Action OnManualSteering;
        public static event Action RieglScanStarting;

        public static event Action LaunchUWB;

        public static event Action StartGeoSama;

        public static event Action StartUwbCalibration;

        #endregion

        void Start()
        {
            // Get the elements from the UI
            var root = UIDocument.rootVisualElement;

            // Bot Panel
            closeButton = root.Q<Button>("CloseButton");
            debugButton = root.Q<Button>("DebugButton");
            errorLabel = root.Q<Label>("ErrorLabel");
            errorMessagePanel = root.Q<VisualElement>("ErrorMessagePanel");

            mainView = root.Q<VisualElement>("MainView");
            secondView = root.Q<VisualElement>("secondView");
            cameraWindowCharlie = root.Q<VisualElement>("CameraWindowCharlie");
            cameraWindowLars = root.Q<VisualElement>("CameraWindowLars");

            // Start-Stop Buttons
            commandButton = root.Q<Button>("bStartDrive");
            stopButton = root.Q<Button>("stopButton");

            // Manual Drive Buttons
            driveMode = root.Q<Button>("driveMode");
            turnMode = root.Q<Button>("angleMode");
            forwardButton = root.Q<Button>("forwardButton");
            backwardButton = root.Q<Button>("backwardButton");
            turnCWButton = root.Q<Button>("turnCWButton");
            turnCCWButton = root.Q<Button>("turnCCWButton");
            incrementButton = root.Q<Button>("incrementButton");
            decrementButton = root.Q<Button>("decrementButton");
            inputTypeLabel = root.Q<Label>("inputTypeLabel");
            valueLabel = root.Q<Label>("value");
            unitLabel = root.Q<Label>("unit");
            stepsizeMin = root.Q<Button>("stepsizeMin");
            stepsizeMid = root.Q<Button>("stepsizeMid");
            stepsizeMax = root.Q<Button>("stepsizeMax");
            resetStepsizeButton = root.Q<Button>("ResetStepsize");

            // Mode Buttons at the Top
            autoDriveModeButton = root.Q<Button>("AutoDriveMode");
            manualDriveModeButton = root.Q<Button>("ManualDriveMode");
            missionModeUWBButton = root.Q<Button>("MissionModeUWB");
            missionModeGeoSAMAButton = root.Q<Button>("MissionModeGeoSAMA");
            scanModeButton = root.Q<Button>("ScanModeButton");
            changeRobotDropdown = root.Q<EnumField>("RobotChoice");

            //Riegl Panel
            startRieglScanButton = root.Q<Button>("StartRieglScanButton");
            scanProgressRiegl = root.Q<ProgressBar>("ScanProgressRiegl");
            timePasedLabel = root.Q<Label>("timePassedLabel");
            rieglCautionPanel = root.Q<VisualElement>("RieglScanCaution");


            // Panels for the different modes (on the right)
            manualDrivePanel = root.Q<VisualElement>("ManualDrivePanel");
            autoDrivePanel = root.Q<VisualElement>("AutoDrivePanel");
            UWBPanel = root.Q<VisualElement>("UWBPanel");
            GeoSAMAPanel = root.Q<VisualElement>("GeoSAMAPanel");
            driveStopPanel = root.Q<VisualElement>("DriveStopPanel");
            rieglPanel = root.Q<VisualElement>("RieglPanel");


            // Bot Panel
            connectionState = root.Q<VisualElement>("ConnectionState");
            connectionLabel = root.Q<Label>("ConnectionLabel");

            // Auto Drive Panel 
            distanceLabel = root.Q<Label>("DistanceLabel");
            arrivalLabel = root.Q<Label>("ArrivalLabel");

            // Main view Panel
            hideModel = root.Q<Button>("HideModel");
            switchViewButton = root.Q<Button>("switchView");
            hideCamera = root.Q<Button>("HideCamera");

            // UWB Panel
            launchButton = root.Q<Button>("LaunchButton");
            trigger1Button = root.Q<Button>("Trigger1");
            trigger2Button = root.Q<Button>("Trigger2");
            trigger3Button = root.Q<Button>("Trigger3");
            trigger4Button = root.Q<Button>("Trigger4");
            rotate45Button = root.Q<Button>("Rotate45Deg");
            rotate90Button = root.Q<Button>("Rotate90Deg");
            popupWindow = root.Q<PopupWindow>("PopupWindow"); // for UWB
            calibrateButton = root.Q<Button>("CalibrateButton");

            // GeoSAMA Panel
            startScanButton = root.Q<Button>("StartScan");
            scanProgressBar = root.Q<ProgressBar>("ScanProgress");

            closeButton.clicked += () =>
            {
                popupWindow.text.text = "Are you sure you want to close the application?";
                ShowPopupWindow();
                lastClickedButton = closeButton;
            };

            debugButton.clicked += () =>
            {
                connectionController.rosConnection.ShowHud = !connectionController.rosConnection.ShowHud;
                connectionController.rosConnection.InitializeHUD();
            };

            hideCamera.clicked += () =>
            {
                if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Charlie)
                {
                    cameraWindowCharlie.style.display = cameraWindowCharlie.style.display == DisplayStyle.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }
                else
                {
                    cameraWindowLars.style.display = cameraWindowLars.style.display == DisplayStyle.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }

                hideCamera.text = hideCamera.text.Equals("Show Camera") ? "Hide Camera" : "Show Camera";
            };

            // Click on mainview, screenpoint is converted to worldpoint
            mainView.RegisterCallback<ClickEvent>(ScreenToWorld);

            // Clicked methods for button //
            commandButton.clicked += () => { SendSteeringCommand(); };
            commandButton.SetEnabled(false); //Cannot be clicked at beginning, need to set goal first 

            trigger1Button.clicked += () =>
            {
                HidePopupWindow();
                SetActiveTriggerButton(trigger1Button);
                popupWindow.text.text = "UWB-1 has been launched before! Are you sure you want to launch it again?";
                lastClickedButton = trigger1Button;
            };
            trigger2Button.clicked += () =>
            {
                HidePopupWindow();
                SetActiveTriggerButton(trigger2Button);
                lastClickedButton = trigger2Button;
                popupWindow.text.text = "UWB-2 has been launched before! Are you sure you want to launch it again?";
            };
            trigger3Button.clicked += () =>
            {
                HidePopupWindow();
                SetActiveTriggerButton(trigger3Button);
                lastClickedButton = trigger3Button;
                popupWindow.text.text = "UWB-3 has been launched before! Are you sure you want to launch it again?";
            };
            trigger4Button.clicked += () =>
            {
                HidePopupWindow();
                SetActiveTriggerButton(trigger4Button);
                lastClickedButton = trigger4Button;
                popupWindow.text.text = "UWB-4 has been launched before! Are you sure you want to launch it again?";
            };

            launchButton.clicked += () => { LaunchActiveUWB(); };
            launchButton.SetEnabled(false); //Cannot be clicked at beginning, need to select UWB Sensor first

            // For UWB
            startScanButton.clicked += () =>
            {
                StartScan();
                StartGeoSama?.Invoke();
            };

            rotate45Button.clicked += () =>
            {
                SetStearingInformation(turnCCWButton);
                Robot.Instance.tempAngle = Robot.Instance.Angle;
                Robot.Instance.Angle = 45;
                OnManualSteering?.Invoke();
            };

            rotate90Button.clicked += () =>
            {
                SetStearingInformation(turnCCWButton);
                Robot.Instance.tempAngle = Robot.Instance.Angle;
                Robot.Instance.Angle = 90;
                OnManualSteering?.Invoke();
            };

            calibrateButton.clicked += () =>
            {
                ShowPopupWindow();
                popupWindow.text.text = "Robot will drive 2 meters forward! Please Start Recording on the Laptop.";
                lastClickedButton = calibrateButton;
            };

            // For Riegl
            startRieglScanButton.clicked += () => { StartRieglScan(); };

            popupWindow.confirmed += () =>
            {
                // If button clicked is "Trigger1 or 2 or 3 or 4"
                if (lastClickedButton == calibrateButton)
                {
                    Debug.Log("Calibrate Process started");
                    HidePopupWindow();
                    StartUwbCalibration?.Invoke();
                }
                else if (lastClickedButton == closeButton)
                {
                    HidePopupWindow();
                    Application.Quit();
                } else if (lastClickedButton == confirmedUwbCalibDone)
                {
                    HidePopupWindow();
                }
                else
                {
                    EnableLaunch();
                    Debug.Log("Launch enabled");
                }
            };
            
            popupWindow.canceled += () =>
            {
                if (lastClickedButton != calibrateButton) DisableLaunch();
                else HidePopupWindow();
            };

            stopButton.clicked += () => { ExecuteStopButtonCommand(); };

            driveMode.clicked += () =>
            {
                ChangeManualMode(driveMode);
                ChangeStartDrivingButton(false);
                ChangeStepSize(stepsizeMid);
                SetActiveButtonCSS(stepsizeMid, new Button[] { stepsizeMin, stepsizeMax });
                CheckShowResetButton();
            };

            turnMode.clicked += () =>
            {
                ChangeManualMode(turnMode);
                ChangeStartDrivingButton(false);
                ChangeStepSize(stepsizeMid);
                SetActiveButtonCSS(stepsizeMid, new Button[] { stepsizeMin, stepsizeMax });
                CheckShowResetButton();
            };

            stepsizeMin.clicked += () =>
            {
                ChangeStepSize(stepsizeMin);
                SetActiveButtonCSS(stepsizeMin, new Button[] { stepsizeMid, stepsizeMax });
            };

            stepsizeMid.clicked += () =>
            {
                ChangeStepSize(stepsizeMid);
                SetActiveButtonCSS(stepsizeMid, new Button[] { stepsizeMin, stepsizeMax });
            };

            stepsizeMax.clicked += () =>
            {
                ChangeStepSize(stepsizeMax);
                SetActiveButtonCSS(stepsizeMax, new Button[] { stepsizeMin, stepsizeMid });
            };

            forwardButton.clicked += () =>
            {
                SetStearingInformation(forwardButton);
                ChangeStartDrivingButton(true);
                SetActiveButtonCSS(forwardButton, new Button[] { backwardButton });
            };

            backwardButton.clicked += () =>
            {
                SetStearingInformation(backwardButton);
                ChangeStartDrivingButton(true);
                SetActiveButtonCSS(backwardButton, new Button[] { forwardButton });
            };

            turnCWButton.clicked += () =>
            {
                SetStearingInformation(turnCWButton);
                ChangeStartDrivingButton(true);
                SetActiveButtonCSS(turnCWButton, new Button[] { turnCCWButton });
            };

            turnCCWButton.clicked += () =>
            {
                SetStearingInformation(turnCCWButton);
                ChangeStartDrivingButton(true);
                SetActiveButtonCSS(turnCCWButton, new Button[] { turnCWButton });
            };


            resetStepsizeButton.clicked += () => { ResetStepsize(); };

            incrementButton.clicked += () => { IncrementAngleDuration(); };
            decrementButton.clicked += () => { DecrementAngleDuration(); };

            autoDriveModeButton.clicked += () =>
            {
                SetOperationMode(Robot.OperationMode.autoDrive);
                ChangeStartDrivingButton(false);
            };
            manualDriveModeButton.clicked += () =>
            {
                SetOperationMode(Robot.OperationMode.manualDrive);
                ChangeStartDrivingButton(false);
                SetActiveButtonCSS(null, new Button[] { forwardButton, backwardButton, turnCWButton, turnCCWButton });
            };
            missionModeUWBButton.clicked += () => { SetOperationMode(Robot.OperationMode.uwbMission); };
            missionModeGeoSAMAButton.clicked += () => { SetOperationMode(Robot.OperationMode.geoSamaMission); };
            scanModeButton.clicked += () => { SetOperationMode(Robot.OperationMode.rieglScan); };

            switchViewButton.clicked += () => { SwitchView(); };

            hideModel.clicked += () => { toggleModelVisability(); };

            //Method when Dropdown Menue is clicked
            changeRobotDropdown.RegisterValueChangedCallback(evt => { ChangeActiveRobotDropdown(evt.newValue); });

            connectionController.OnConnectionStatusChanged += HandleConnectionStatusChanged;
            StartCoroutine(InitializeAfterLayout());
            //At the beginning, the decrement button should be inactive because the distance is 0 and the angle is 0 
            UpdateButtonStates();
            cameraWindowCharlie.style.display = DisplayStyle.None;
            cameraWindowLars.style.display = DisplayStyle.None;

            //connectionController.rosConnection.HUDPanel.gameObject.SetActive(showHud);
        }

        /*
         * Notifier when UWB calibration is finished
         */
        public void NotifyFinishedUwbCalibration()
        {
            ShowPopupWindow();
            popupWindow.text.text = "Calibration finished. Please start moving the robot inside the area.";
            lastClickedButton = confirmedUwbCalibDone;
        }

        /*
         * Method to set the active trigger button and disable the other trigger buttons.
         * Return if button clicked again but still executing the flashing of the button, so button cannot be clicked more times in a row.
         */
        private void ExecuteStopButtonCommand()
        {
            if (stopButtonClicked) return;
            SetStearingInformation(stopButton);
            ManualSteeringButtonClicked();
            FlashStopButton();
            ChangeMarkerColor(false);
            ChangeStartDrivingButton(false);
            SetActiveButtonCSS(null, new Button[] { forwardButton, backwardButton, turnCWButton, turnCCWButton });
        }

        /// <summary>
        /// Updates the error label and its background color in the UI based on the error count and error message.
        /// </summary>
        /// <param name="errorCount">The number of errors.</param>
        /// <param name="errorMessage">The error message.</param>
        public void UpdateErrorLabel(int errorCount, string errorMessage)
        {
            // If there are errors
            if (errorCount > 0)
            {
                // Set the error message panel background color to red
                errorMessagePanel.style.backgroundColor = Color.red;

                // If the error message contains "Camera"
                if (errorMessage.Contains("Camera"))
                {
                    // Set the error label text to "Error: Camera"
                    errorLabel.text = "Error: Camera";
                }
                // If the error message contains "Laser"
                else if (errorMessage.Contains("Laser"))
                {
                    // Set the error label text to "Error: Laser"
                    errorLabel.text = "Error: Laser";
                }
                else
                {
                    // If the error message does not contain "Camera" or "Laser", set the error label text to "Error: Unknown"
                    errorLabel.text = "Error: Unknown";
                }
            }
            else
            {
                // If there are no errors, set the error message panel background color to green and the error label text to "No Errors"
                errorMessagePanel.style.backgroundColor = greenButtonColor;
                errorLabel.text = "No Errors";
            }
        }

        public void RenderRealsenseCamera(CompressedImageMsg msg)
        {
            Texture2D tex2D = msg.ToTexture2D();
            //tex2D = rotateTexture(tex2D, true);
            RenderTexture renderTex = RenderTexture.GetTemporary(
                tex2D.width,
                tex2D.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(tex2D, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            imageTextureRealsense.Release();
            Graphics.Blit(tex2D, imageTextureRealsense);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            Destroy(tex2D);
        }

        public void RenderGeoSamaCamera(CompressedImageMsg msg)
        {
            Texture2D tex2D = msg.ToTexture2D();
            tex2D = rotateTexture(tex2D, true);
            RenderTexture renderTex = RenderTexture.GetTemporary(
                tex2D.width,
                tex2D.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(tex2D, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            imageTextureGeoSAMA.Release();
            Graphics.Blit(tex2D, imageTextureGeoSAMA);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            Destroy(tex2D);
        }

        Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        private void ShowPopupWindow()
        {
            popupWindow.style.display = DisplayStyle.Flex;
        }

        private void HidePopupWindow()
        {
            popupWindow.style.display = DisplayStyle.None;
        }


        private void StartRieglScan()
        {
            if (Robot.Instance._operationMode == Robot.OperationMode.rieglScan)
            {
                RieglScanStarting?.Invoke();
                StartScan();
                UpdateTimePassedLabel();
                rieglCautionPanel.style.display = DisplayStyle.Flex;
            }
        }

        /*
         * Update the time passed label in the UI.
         * Start by 6 mins and count down to 0 in the format mm:ss
         */
        public void UpdateTimePassedLabel()
        {
            StartCoroutine(UpdateTimePassedLabelCoroutine());
        }

        private IEnumerator UpdateTimePassedLabelCoroutine()
        {
            int time = 300;
            while (time > 0)
            {
                int minutes = time / 60;
                int seconds = time % 60;
                string timeString = minutes.ToString("00") + ":" + seconds.ToString("00");
                timePasedLabel.text = timeString;
                time--;
                yield return new WaitForSeconds(1);
            }

            timePasedLabel.text = "00:00";
        }

        /*
         * Method to reset the UI to the default state, needed when active robot switched
         */
        public void ResetUI()
        {
            StartCoroutine(ResetUICoroutine());
        }

        private IEnumerator ResetUICoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
            {
                missionModeUWBButton.style.display = DisplayStyle.None;
                missionModeGeoSAMAButton.style.display = DisplayStyle.None;
                scanModeButton.style.display = DisplayStyle.Flex;
                cameraWindowCharlie.style.display = DisplayStyle.None;
                cameraWindowLars.style.display = DisplayStyle.None;
            }
            else
            {
                missionModeUWBButton.style.display = DisplayStyle.Flex;
                missionModeGeoSAMAButton.style.display = DisplayStyle.Flex;
                scanModeButton.style.display = DisplayStyle.None;
                cameraWindowCharlie.style.display = DisplayStyle.None;
                cameraWindowLars.style.display = DisplayStyle.None;
            }

            hideCamera.text = "Show Camera";
            marker.transform.position = new Vector3(Robot.Instance.Robot3DModel.transform.position.x, 0f,
                Robot.Instance.Robot3DModel.transform.position.z);
            //Reset the Operation Mode and show the auto drive panel
            Robot.Instance._operationMode = Robot.OperationMode.autoDrive;
            SetOperationMode(Robot.Instance._operationMode);

            //Reset the Distance Label
            distanceLabel.text = "0 m";

            //Reset the Arrival Label
            arrivalLabel.text = "0 m";

            //Reset the Active View to the Main View
            if (!isMainActive)
                SwitchView();

            //Reset the Duration 
            valueLabel.text = "0";
            Robot.Instance.Distance = 0;

            //Reset the FOV
            cameraController.ResetCameraFOV();

            //Reset the Camera Rotation
            cameraController.StartResetCameraRotation();

            // Hide reset FOV / Rotation Button
            cameraController.resetButton.style.display = DisplayStyle.None;
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
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width,
                mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width,
                secondView.resolvedStyle.height);
            // Unsubscribe when the application is closed
            connectionController.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
            imageTextureRealsense.Release();
            imageTextureGeoSAMA.Release();
        }

        /*
         * Change the color of the connection state in the UI depending on the connection status
         */
        private void HandleConnectionStatusChanged(bool isConnected)
        {
            // Update the connection state in the UI
            if (isConnected)
            {
                connectionState.style.backgroundColor = new StyleColor(greenButtonColor);
                connectionLabel.text = "Connected";
            }
            else
            {
                connectionState.style.backgroundColor = new StyleColor(Color.red);
                connectionLabel.text = "Disconnected";
            }
        }

        /*
         * Change the active robot depending on the selected value in the dropdown menu
         */
        private void ChangeActiveRobotDropdown(Enum newValue)
        {
            Robot.ACTIVEROBOT selectedRobot = (Robot.ACTIVEROBOT)newValue;
            Robot.Instance.ActiveRobot = selectedRobot;
            connectionController.ChangeRobotIP();
            // The Offset of the second camera is different for Charlie and Lars, because Charlies camera is rotated 90 deg 
            ResetUI();
        }

        private void ChangeManualMode(Button clickedButton)
        {
            if (clickedButton == turnMode)
            {
                Robot.Instance.ManualMode = Robot.MANUALMODE.rotate;
                SetActiveButtonCSS(turnMode, new Button[] { driveMode });
                SetActiveButtonCSS(null, new Button[] { forwardButton, backwardButton });
                turnCWButton.style.display = DisplayStyle.Flex;
                turnCCWButton.style.display = DisplayStyle.Flex;
                forwardButton.style.display = DisplayStyle.None;
                backwardButton.style.display = DisplayStyle.None;
                stepsizeMin.text = "Stepsize\n 1°";
                stepsizeMid.text = "Stepsize\n 5°";
                stepsizeMax.text = "Stepsize\n 45°";
            }
            else
            {
                Robot.Instance.ManualMode = Robot.MANUALMODE.drive;
                SetActiveButtonCSS(driveMode, new Button[] { turnMode });
                SetActiveButtonCSS(null, new Button[] { turnCWButton, turnCCWButton });
                turnCWButton.style.display = DisplayStyle.None;
                turnCCWButton.style.display = DisplayStyle.None;
                forwardButton.style.display = DisplayStyle.Flex;
                backwardButton.style.display = DisplayStyle.Flex;
                stepsizeMin.text = "Stepsize\n 0.1m";
                stepsizeMid.text = "Stepsize\n 0.5m";
                stepsizeMax.text = "Stepsize\n 1m";
            }

            ToggleDurationAngleLabel();
        }

        private void SetActiveButtonCSS(Button selectedButton, Button[] unselectedButtons)
        {
            if (selectedButton != null)
            {
                selectedButton.style.backgroundColor = new StyleColor(greenButtonColor);
            }

            foreach (var button in unselectedButtons)
            {
                button.style.backgroundColor = new StyleColor(unselectedButtonColor);
            }
        }

        /*
         * Wait until the layout is done and then resize the RenderTextures to the correct size
         */
        private IEnumerator InitializeAfterLayout()
        {
            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width,
                mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width,
                secondView.resolvedStyle.height);
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
            if (evt.target != mainView) return;
            clickPosition = evt.localPosition;
            // Convert click position to a proportion of the VisualElement's size (because it is in Pixels and not in World Units)
            clickPosition.x /= mainView.resolvedStyle.width;
            clickPosition.y /= mainView.resolvedStyle.height;

            Vector3
                viewportPoint =
                    new Vector3(clickPosition.x,
                        1 - clickPosition.y); //Invert Y, because (0.0) is bottom left in UI, but top left in camera
            Vector3 worldPosition = new Vector3();
            Ray ray = cameraController.activeMainUICamera.ViewportPointToRay(viewportPoint);

            //Raycast it against ground Plane, shorthand for a vector projection.
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                worldPosition = ray.GetPoint(enter);
                marker.transform.position =
                    new Vector3(worldPosition.x, 0f, worldPosition.z); //Add 0.75f to make above the robot model
                marker.transform.LookAt(cameraController.secondCamera.transform,
                    Vector3.down); //rotate towards the camera
                // Set the world coordinates in the robot model
                Robot.Instance.SetGoalInWorldPos(worldPosition);
                Debug.Log("Clicked Point: " + worldPosition);
            }

            /*
            if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Charlie)
            {
                float tempx = worldPosition.x - Charlie.Instance.CurrentPos.x;
                float tempz = worldPosition.z - Charlie.Instance.CurrentPos.z;

                worldPosition.x = Charlie.Instance.CurrentPos.x + Mathf.Cos((float) Charlie.Instance.theta * Mathf.Deg2Rad) * tempx - Mathf.Sin((float) Charlie.Instance.theta * Mathf.Deg2Rad) * tempz;
                worldPosition.z = Charlie.Instance.CurrentPos.z + Mathf.Sin((float) Charlie.Instance.theta * Mathf.Deg2Rad) * tempx + Mathf.Cos((float) Charlie.Instance.theta * Mathf.Deg2Rad) * tempz;
                Robot.Instance.SetGoalInWorldPos(worldPosition);
            }
            */
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
            float distance = Vector3.Distance(Robot.Instance.Robot3DModel.transform.position, worldPosition);
            distance -= 0.5f; // Subtract the radius of the robot
            Debug.Log("Distance: " + distance + " m");
            UpdateDistanceLabel(distance);
            UpdateArrivalLabel(distance);
        }

        private void UpdateArrivalLabel(float distance = 0)
        {
            if (distance == 0) return;
            // The distance only in 2 decimal places
            distance = (float)Math.Round(distance, 2);
            string distanceString = distance.ToString();
            // Add " m" to the distance string
            distanceString += " m";
            arrivalLabel.text = distanceString;
        }

        void Update()
        {
            UpdateArrivalDistance();
        }

        private void UpdateArrivalDistance()
        {
            // Calculate the distance between the robot and the marker
            if (arrow.GetComponent<MeshRenderer>().material.color == Color.green)
            {
                distance = Vector3.Distance(Robot.Instance.Robot3DModel.transform.position, marker.transform.position) -
                           0.5f;
            }

            // Consider the robot to have arrived if the distance is less than 0.1 units
            if (distance < 0.05f)
            {
                arrivalLabel.text = "0 m";
                ChangeMarkerColor(false);
            }
            else
            {
                // The distance only in 2 decimal places
                distance = (float)Math.Round(distance, 2);
                string distanceString = distance.ToString();
                // Add " m" to the distance string
                distanceString += " m";
                arrivalLabel.text = distanceString;
            }
        }

        private void UpdateDistanceLabel(float distance)
        {
            // The distance only in 2 decimal places
            distance = (float)Math.Round(distance, 2);
            string distanceString = distance.ToString();
            // Add " m" to the distance string
            distanceString += " m";
            distanceLabel.text = distanceString;
        }

        /*
         * Event listener:
         * Waiting for button click event to start driving to the goal
         */
        private void SendSteeringCommand()
        {
            if (Robot.Instance._operationMode == Robot.OperationMode.autoDrive)
            {
                OnStartDriving?.Invoke();
                ChangeStartDrivingButton(false);
                ChangeMarkerColor(true);
            }
            else if (Robot.Instance._operationMode == Robot.OperationMode.manualDrive)
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
            stopButtonClicked = true;
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

            button.style.backgroundColor = Color.red;
            stopButtonClicked = false;
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
            else if (clickedButton == turnCWButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.right;
            }
            else if (clickedButton == turnCCWButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.left;
            }
            else if (clickedButton == stopButton)
            {
                Robot.Instance.Direction = Robot.DIRECTIONS.stop;
            }
        }

        /*
         * Increment the duration of the robot model and update the duration label in the UI
         */
        private void IncrementAngleDuration()
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
            {
                Robot.Instance.IncrementDistance(distanceStepSize);
                UpdateUnitValueLabel(false);
            }
            else
            {
                Robot.Instance.IncrementAngle(turnStepSize);
                Robot.Instance.tempAngle = Robot.Instance.Angle;
                UpdateUnitValueLabel(true);
            }

            UpdateButtonStates();
            CheckShowResetButton();
        }

        /*
         * Decrement the duration of the robot model and update the duration label in the UI
         */
        private void DecrementAngleDuration()
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
            {
                if (Robot.Instance.Distance <= 0) decrementButton.SetEnabled(false);
                else decrementButton.SetEnabled(true);
                Robot.Instance.DecrementDistance(distanceStepSize);
                UpdateUnitValueLabel(false);
            }
            else
            {
                if (Robot.Instance.Angle <= 0) decrementButton.SetEnabled(false);
                else decrementButton.SetEnabled(true);
                Robot.Instance.DecrementAngle(turnStepSize);
                Robot.Instance.tempAngle = Robot.Instance.Angle;
                UpdateUnitValueLabel(true);
            }

            UpdateButtonStates();
            CheckShowResetButton();
        }

        private void UpdateButtonStates()
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
            {
                incrementButton.SetEnabled(Robot.Instance.Distance < 8);
                decrementButton.SetEnabled(Robot.Instance.Distance > 0);
            }
            else
            {
                incrementButton.SetEnabled(Robot.Instance.Angle < 360);
                decrementButton.SetEnabled(Robot.Instance.Angle > 0);
            }
        }

        private void CheckShowResetButton()
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive && Robot.Instance.Distance > 0 ||
                Robot.Instance.ManualMode == Robot.MANUALMODE.rotate && Robot.Instance.Angle > 0)
            {
                resetStepsizeButton.style.display = DisplayStyle.Flex;
            }
            else resetStepsizeButton.style.display = DisplayStyle.None;
        }

        private void ResetStepsize()
        {
            resetStepsizeButton.style.display = DisplayStyle.None;
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
            {
                Robot.Instance.Distance = 0;
                UpdateUnitValueLabel(false);
            }
            else
            {
                Robot.Instance.Angle = 0;
                UpdateUnitValueLabel(true);
            }

            UpdateButtonStates();
        }


        /*
         * Update the duration label in the UI with the duration of the robot model
         */
        private void UpdateUnitValueLabel(bool angle)
        {
            valueLabel.text = angle ? Robot.Instance.Angle.ToString() : Robot.Instance.Distance.ToString();
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
            else if (mode == Robot.OperationMode.rieglScan)
            {
                Robot.Instance._operationMode = Robot.OperationMode.rieglScan;
                SetRieglScanMode();
            }

            ChangeOperationButtonColor();
        }

        private void SetRieglScanMode()
        {
            SetPanelDisplay(Robot.OperationMode.rieglScan);
        }

        /*
         * Set the panel display depending on the operation mode.
         * The drive-stop panel is displayed in manual and auto drive mode
         */
        private void SetPanelDisplay(Robot.OperationMode mode)
        {
            manualDrivePanel.style.display =
                (mode == Robot.OperationMode.manualDrive) ? DisplayStyle.Flex : DisplayStyle.None;
            autoDrivePanel.style.display =
                (mode == Robot.OperationMode.autoDrive) ? DisplayStyle.Flex : DisplayStyle.None;
            UWBPanel.style.display = (mode == Robot.OperationMode.uwbMission) ? DisplayStyle.Flex : DisplayStyle.None;
            GeoSAMAPanel.style.display =
                (mode == Robot.OperationMode.geoSamaMission) ? DisplayStyle.Flex : DisplayStyle.None;
            driveStopPanel.style.display =
                (mode == Robot.OperationMode.manualDrive || mode == Robot.OperationMode.autoDrive)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            rieglPanel.style.display = (mode == Robot.OperationMode.rieglScan) ? DisplayStyle.Flex : DisplayStyle.None;
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
                Charlie.Instance.UwbTrigger = Charlie.UWBTRIGGER.noTrigger;
            }
            else
            {
                // Reset the state of the previously clicked button
                if (activeTriggerButton != null)
                {
                    ResetButtonState(activeTriggerButton);
                }

                // Change the border color of the clicked button
                clickedButton.style.borderBottomColor = greenButtonColor;
                clickedButton.style.borderTopColor = greenButtonColor;
                clickedButton.style.borderLeftColor = greenButtonColor;
                clickedButton.style.borderRightColor = greenButtonColor;

                // Update the last clicked button
                activeTriggerButton = clickedButton;

                // Set the active UWB in the robot model
                SetActiveUWB();

                if (activeTriggerButton.style.backgroundColor == disabledButtonColor)
                {
                    //launchButton.text = "UWB already launched, try again?";
                    //launchButton.style.backgroundColor = new StyleColor(Color.yellow);
                    ShowPopupWindow();
                    launchButton.SetEnabled(false);
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
                Charlie.Instance.UwbTrigger = Charlie.UWBTRIGGER.trigger1;
            }
            else if (activeTriggerButton.name == "Trigger2")
            {
                Charlie.Instance.UwbTrigger = Charlie.UWBTRIGGER.trigger2;
            }
            else if (activeTriggerButton.name == "Trigger3")
            {
                Charlie.Instance.UwbTrigger = Charlie.UWBTRIGGER.trigger3;
            }
            else if (activeTriggerButton.name == "Trigger4")
            {
                Charlie.Instance.UwbTrigger = Charlie.UWBTRIGGER.trigger4;
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
            Debug.Log("Launching: " + Charlie.Instance.UwbTrigger);
            launchButton.SetEnabled(false);
            LaunchUWB?.Invoke();
        }

        /*
         * Start the coroutine to fill the progress bar
         */
        public void StartScan()
        {
            StartCoroutine(FillProgressBar());
        }


        /*
         * If the popup is confirmed, enable the launch button again
         */
        private void EnableLaunch()
        {
            launchButton.SetEnabled(true);
            HidePopupWindow();
        }

        /*
         * If the popup is canceled, disable the launch button
         */
        private void DisableLaunch()
        {
            launchButton.SetEnabled(false);
            HidePopupWindow();
        }

        /*
         * Change the Duration label in the UI to Angle if pressing left or right
         */
        private void ToggleDurationAngleLabel()
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.rotate)
            {
                inputTypeLabel.text = "Angle:";
                valueLabel.text = Robot.Instance.Angle.ToString();
                unitLabel.text = "deg";
            }
            else
            {
                inputTypeLabel.text = "Distance:";
                valueLabel.text = Robot.Instance.Distance.ToString();
                unitLabel.text = "m";
            }
        }

        private void ChangeStepSize(Button clickedButton)
        {
            if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
            {
                if (clickedButton == stepsizeMin)
                {
                    distanceStepSize = 0.1f;
                }
                else if (clickedButton == stepsizeMid)
                {
                    distanceStepSize = 0.5f;
                }
                else if (clickedButton == stepsizeMax)
                {
                    distanceStepSize = 1f;
                }
            }
            else
            {
                if (clickedButton == stepsizeMin)
                {
                    turnStepSize = 1f;
                }
                else if (clickedButton == stepsizeMid)
                {
                    turnStepSize = 5f;
                }
                else if (clickedButton == stepsizeMax)
                {
                    turnStepSize = 45f;
                }
            }
        }

        #region USS Styling Methods

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
                { Robot.OperationMode.geoSamaMission, missionModeGeoSAMAButton },
                { Robot.OperationMode.rieglScan, scanModeButton }
            };

            // Iterate through each mode-button pair
            foreach (var pair in modeToButton)
            {
                pair.Value.style.backgroundColor =
                    new StyleColor(pair.Key == mode ? greenButtonColor : blueButtonColor);
            }
        }

        /*
         * Coroutine to fill the progress bar
         */
        private IEnumerator FillProgressBar()
        {
            // On Charlie start GeoSAMA scan (5 seconds)
            if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Charlie)
            {
                scanProgressBar.value = 0;
                scanProgressBar.title = "Scanning...";
                float duration = 5;
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
            // On Lars start Riegl Scan (5 mins)
            else
            {
                scanProgressRiegl.value = 0;
                scanProgressRiegl.title = "Scanning...";
                float duration = 300;
                float timePassed = 0f;
                while (timePassed < duration)
                {
                    timePassed += Time.deltaTime;
                    scanProgressRiegl.value = (timePassed / duration) * scanProgressRiegl.highValue;
                    yield return null;
                }

                scanProgressRiegl.value = scanProgressRiegl.highValue;
                // Set text to "Scan finished" from the progress bar
                scanProgressRiegl.title = "Scan finished";
                rieglCautionPanel.style.display = DisplayStyle.None;
            }
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
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width,
                mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width,
                secondView.resolvedStyle.height);
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