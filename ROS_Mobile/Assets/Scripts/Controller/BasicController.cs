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
    private QuaternionMsg qMessageCharlie = new QuaternionMsg();
    //PointMsg pMessage = new PointMsg();
    
    #endregion
    
    
    void Start()
    {
        robot = Robot.Instance;
        // Subscribe to the current pose of the robot
        rosConnection.Subscribe<OdometryMsg>("cur_pose", msg =>
        {
            //pMessage = msg.pose.pose.position;
            Lars.Instance.CurrentX = msg.pose.pose.position.x;
            Lars.Instance.CurrentY = msg.pose.pose.position.y;
            Lars.Instance.CurrentZ = msg.pose.pose.position.z;
            qMessage = msg.pose.pose.orientation;
        });
        
        rosConnection.Subscribe<OdometryMsg>("/base_controller_node/odom", msg =>
        {
            //pMessage = msg.pose.pose.position;
            Charlie.Instance.CurrentX = msg.pose.pose.position.x;
            Charlie.Instance.CurrentY = msg.pose.pose.position.y;
            Charlie.Instance.CurrentZ = msg.pose.pose.position.z;
            qMessageCharlie = msg.pose.pose.orientation;
        }
        );
        //rosConnection.Subscribe<OdometryMsg>("pos");
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
        if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
        {
            robot3DModel.transform.position = new Vector3(Lars.Instance.CurrentPos.x, -0.54f, Lars.Instance.CurrentPos.z); //Make that it is on the ground and not in the air
            robot3DModel.transform.rotation = Lars.Instance.CurrentRot;
        }
        else
        { 
            robot3DModel.transform.position = new Vector3(Charlie.Instance.CurrentPos.x, -0.54f, Charlie.Instance.CurrentPos.z); //Make that it is on the ground and not in the air
            Vector3 angles = new Vector3(0, 90, 0);
            Quaternion temp = new Quaternion();
            temp.eulerAngles = angles;
            robot3DModel.transform.rotation = temp;
        }
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
        Lars.Instance.CurrentRot = robotAngle;
        
        
        Quaternion quadCharlie = new Quaternion((float) qMessageCharlie.x, (float) qMessageCharlie.y,(float) qMessageCharlie.z, (float)qMessageCharlie.w);
       
        Charlie.Instance.theta = quadCharlie.eulerAngles.z;
        
        //Debug.Log("Rot: " + Charlie.Instance.CurrentRot);
    }
    
    /*
     * Update the position value of the robot model
     */
    private void UpdateRobotPosition()
    {
        //robot.currentPos = new Vector3((float) - pMessage.y, (float) pMessage.z, (float) pMessage.x);
        Lars.Instance.CurrentPos = new Vector3((float)- Lars.Instance.CurrentY,(float) Lars.Instance.CurrentZ, (float) Lars.Instance.CurrentX);
        Charlie.Instance.CurrentPos =  new Vector3((float) - Charlie.Instance.CurrentX,(float) Charlie.Instance.CurrentZ, (float) - Charlie.Instance.CurrentY);
        //Debug.Log("Pos: " + Charlie.Instance.CurrentPos);
       // Robot.Instance.CurrentPos = robot.CurrentPos;
    }
}
