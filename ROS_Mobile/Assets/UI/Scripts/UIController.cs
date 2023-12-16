using System;
using System.Collections;
using Model;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
namespace myUIController
{

    public class UIController : MonoBehaviour
    {
        #region VisualElements

        private VisualElement mainView; //View which shows RenderTexture from Camera 

        private VisualElement secondView; //View which shows Top-Down view of the map

        private VisualElement manualDrivePanel;
        
        private VisualElement autoDrivePanel;
        

        #endregion

        #region Buttons

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



        #endregion

        private Label speedLabel;
        private Camera mainCamera;
        private Camera secondCamera;
        

        private Vector3 clickPosition;
        private bool isMainActive = true;

        [SerializeField] public GameObject marker;
        [SerializeField] public RenderTexture mainViewTexture;
        [SerializeField] public RenderTexture secondViewTexture;
        
        CameraController cameraController;
        

        // Event is triggered when Button is Clicked
        public static event Action OnStartDriving;
        public static event Action OnManualSteering;

        void Start()
        {
            cameraController = GetComponent<CameraController>();
          
            // Get the elements from the UI
            var root = GetComponent<UIDocument>().rootVisualElement;
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
            speedLabel = root.Q<Label>("Time");
            autoDriveModeButton = root.Q<Button>("AutoDriveMode");
            manualDriveModeButton = root.Q<Button>("ManualDriveMode");
            manualDrivePanel = root.Q<VisualElement>("ManualDrivePanel");
            autoDrivePanel = root.Q<VisualElement>("AutoDrivePanel");
            switchViewButton = root.Q<Button>("switchView");
          
            
            
            // Click on mainview, screenpoint is converted to worldpoint
            mainView.RegisterCallback<ClickEvent>(screenToWorld);

            // Clicked methods for button
            driveButton.clicked += () => { StartDrivingButtonClicked(); };
            stopButton.clicked += () =>
            {
                setStearingInformation(stopButton);
                ManualSteeringButtonClicked();
            };
            forwardButton.clicked += () =>
            {
                setStearingInformation(forwardButton);
                ManualSteeringButtonClicked();
            };
            backwardButton.clicked += () =>
            {
                setStearingInformation(backwardButton);
                ManualSteeringButtonClicked();
            };
            leftButton.clicked += () =>
            {
                setStearingInformation(leftButton);
                ManualSteeringButtonClicked();
            };
            rightButton.clicked += () =>
            {
                setStearingInformation(rightButton);
                ManualSteeringButtonClicked();
            };
            
            autoDriveModeButton.clicked += () => { setAutoDriveMode(); };
            manualDriveModeButton.clicked += () => { setManualDriveMode(); };
            incrementButton.clicked += () => { incrementSpeed(); };
            decrementButton.clicked += () => { decrementSpeed(); };
            
            switchViewButton.clicked += () => { switchView(); };
            
            StartCoroutine(InitializeAfterLayout());
           
        }
        
        private IEnumerator InitializeAfterLayout()
        {
            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
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
        }

        /*
         * Code adapted from:
         * https://forum.unity.com/threads/solved-how-to-get-world-point-coordinates-from-a-render-texture.539393/
         */
        private void screenToWorld(ClickEvent evt)
        {
            clickPosition = evt.localPosition;
            // Convert click position to a proportion of the VisualElement's size (because it is in Pixels and not in World Units)
            clickPosition.x /= mainView.resolvedStyle.width;
            clickPosition.y /= mainView.resolvedStyle.height;

            Vector3
                viewportPoint =
                    new Vector3(clickPosition.x,
                        1 - clickPosition.y); //Invert Y, because (0.0) is bottom left in UI, but top left in camera

            Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
            /*
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Get the point where it hit
                Vector3 mousePositionWorld = hit.point;
                marker.transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
                Debug.DrawLine(ray.origin, hit.point, Color.green, 5.0f);
                Debug.LogWarning(hit.collider.gameObject.name);
                setWorldCoordinates(mousePositionWorld);
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 5.0f);
            }
            */
            
            //Raycast it against ground Plane, shorthand for a vector projection. Works better than the above code
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPosition = ray.GetPoint(enter);
                marker.transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
                //Debug.Log("World position: " + new Vector3(worldPosition.z, - worldPosition.x, worldPosition.y));   
                //Debug.DrawLine(ray.origin, ray.GetPoint(enter), Color.green, 5.0f);

                // Set the world coordinates in the robot model
                Robot.Instance.SetWorldPosition(worldPosition);
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 5.0f);
            }

        }

        private void StartDrivingButtonClicked()
        {
            OnStartDriving?.Invoke();
        }

        private void ManualSteeringButtonClicked()
        {
            OnManualSteering?.Invoke();
        }

        private void setStearingInformation(Button clickedButton)
        {
            UnityEngine.Debug.Log("height: " + secondView.resolvedStyle.height + "\n width: " + secondView.resolvedStyle.width);
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

        private void incrementSpeed()
        {
            Robot.Instance.incrementSpeed();
            updateSpeedLabelInView();
        }
        
        private void decrementSpeed()
        {
            Robot.Instance.decrementSpeed();
          updateSpeedLabelInView();
        }
        
        private void updateSpeedLabelInView()
        {
            speedLabel.text = Robot.Instance.Speed.ToString(); // Update the speed label in the UI
        }

        private void setManualDriveMode()
        {
            manualDrivePanel.style.display = DisplayStyle.Flex;
            autoDrivePanel.style.display = DisplayStyle.None;
        }
        
        private void setAutoDriveMode()
        {
            autoDrivePanel.style.display = DisplayStyle.Flex;
            manualDrivePanel.style.display = DisplayStyle.None; 
        }
        
        private void switchView()
        {
            // Toggle the view state
            isMainActive = !isMainActive;

            // Swap textures references
            SwapTextures();

            // Resize textures based on the new active view
            ResizeTextures();

            // Update background images
            UpdateBackgroundImages();

            // Swap camera tags
            cameraController.SwapCameraTags(isMainActive);
        }

        private void SwapTextures()
        {
            (mainViewTexture, secondViewTexture) = (secondViewTexture, mainViewTexture);
            //RenderTexture temp = mainViewTexture;
            //mainViewTexture = secondViewTexture;
            //secondViewTexture = temp;
        }

        private void ResizeTextures()
        {
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
        }

        private void UpdateBackgroundImages()
        {
            mainView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(mainViewTexture));
            secondView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(secondViewTexture));
        }

     


        /*
        private void switchView()
        {
            if (isMainActive)
            {
                SwitchToSecondView();
            }
            else
            {
                SwitchToMainView();
            }
            isMainActive = !isMainActive;
        }

        private void SwitchToSecondView()
        {
            ResizeRenderTextures(secondView.resolvedStyle.width, secondView.resolvedStyle.height, mainView.resolvedStyle.width, mainView.resolvedStyle.height);
            SwapBackgroundImages(secondViewTexture, mainViewTexture);
            SwapCameraTags("SecondCamera", "MainCamera");
        }

        private void SwitchToMainView()
        {
            ResizeRenderTextures(mainView.resolvedStyle.width, mainView.resolvedStyle.height, secondView.resolvedStyle.width, secondView.resolvedStyle.height);
            SwapBackgroundImages(mainViewTexture, secondViewTexture);
            SwapCameraTags("MainCamera", "SecondCamera");
        }

        private void ResizeRenderTextures(float mainWidth, float mainHeight, float secondWidth, float secondHeight)
        {
            mainViewTexture = RenderTextureResize.Resize(mainViewTexture, mainWidth, mainHeight);
            secondViewTexture = RenderTextureResize.Resize(secondViewTexture, secondWidth, secondHeight);
        }

        private void SwapBackgroundImages(RenderTexture mainTexture, RenderTexture secondTexture)
        {
            mainView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(mainTexture));
            secondView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(secondTexture));
        }

        private void SwapCameraTags(string mainTag, string secondTag)
        {
            mainCamera.tag = mainTag;
            secondCamera.tag = secondTag;
        }

*/
    }
    
}

