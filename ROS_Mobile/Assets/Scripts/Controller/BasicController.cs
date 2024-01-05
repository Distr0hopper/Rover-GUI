using UnityEngine;
using Model;
using myUIController;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UIElements;

public class BasicController : MonoBehaviour
{
    #region Serialized Fields
    
    [SerializeField] private UIDocument UIDocument;
    [SerializeField] public GameObject robot3DModel;
    
    #endregion

    #region Private Fields
    

    private Robot robot;
    private ROSConnection rosConnection {get; set;}
    private QuaternionMsg qMessage = new QuaternionMsg();
    //PointMsg pMessage = new PointMsg();
    
    #endregion

    #region Enums

    public enum ACTIVEROBOT
    {
        Charlie = 0,
        Lars = 1
    }

    public static ACTIVEROBOT ActiveRobot { get; set; } = ACTIVEROBOT.Charlie;

    #endregion
    
    
    void Start()
    {
        // Subscribe to the current pose of the robot
        rosConnection.Subscribe<OdometryMsg>("cur_pose", msg =>
        {
            //pMessage = msg.pose.pose.position;
            robot.CurrentX = msg.pose.pose.position.x;
            robot.CurrentY = msg.pose.pose.position.y;
            robot.CurrentZ = msg.pose.pose.position.z;
            qMessage = msg.pose.pose.orientation;
        });
    }

    private void Awake()
    {
        robot = new Robot();
        rosConnection = ROSConnection.GetOrCreateInstance();
        // Getting all controllers 
        var rosSender = FindObjectOfType<ROSSender>();
        var cameraController = FindObjectOfType<CameraController>();
        var uiController = FindObjectOfType<UIController>();
        var connectionController = FindObjectOfType<ConnectionController>();
        
        rosSender.rosConnection = rosConnection;

        cameraController.UIDocument = UIDocument;

        uiController.UIDocument = UIDocument;
        uiController.cameraController = cameraController;
        uiController.connectionController = connectionController;
        
        
        connectionController.rosConnection = rosConnection;
        Robot.Instance.Robot3DModel = robot3DModel;
    }
    
    void Update()
    {
        UpdateRobotPosition();
        UpdateRobotRotation();
        Update3DModelInScene();
    }
    

    /*
     * Set the position and the rotation of the robot model in the scene to the current position and rotation of the robot 
     */
    private void Update3DModelInScene()
    {
        //robot3DModel.transform.localPosition = robot.currentPos;
        robot3DModel.transform.position = new Vector3(robot.CurrentPos.x, robot.CurrentPos.y, robot.CurrentPos.z); //Make that it is on the ground and not in the air
        robot3DModel.transform.rotation = robot.CurrentRot;
    }

    /*
     * Update the rotation value of the robot model
     */
    private void UpdateRobotRotation()
    {
        Quaternion quad = new Quaternion((float) qMessage.x, (float) qMessage.y,(float) qMessage.z, (float)qMessage.w);
        Vector3 angles = quad.eulerAngles;
        angles.x = 0;
        angles.y = -angles.z;
        angles.z = 0;
        Quaternion robotAngle = new Quaternion();
        robotAngle.eulerAngles = angles;
        robot.CurrentRot = robotAngle;
    }
    
    /*
     * Update the position value of the robot model
     */
    private void UpdateRobotPosition()
    {
        //robot.currentPos = new Vector3((float) - pMessage.y, (float) pMessage.z, (float) pMessage.x);
        robot.CurrentPos = new Vector3((float)- robot.CurrentY,(float) robot.CurrentZ, (float) robot.CurrentX);
    }
}
