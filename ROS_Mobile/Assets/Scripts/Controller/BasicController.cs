using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] public string followTargetName = "base_link";
    [SerializeField] private UIDocument UIDocument;
    [SerializeField] public GameObject robot3DModel;
    
    #endregion

    #region Private Fields
    

    private Robot robot;
    private ROSConnection rosConnection {get; set;}
    private QuaternionMsg qMessage = new QuaternionMsg();
    //PointMsg pMessage = new PointMsg();
    
    #endregion

    #region Public Fields

    public string selectedRobot { get; set; } = "Charlie";
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
            robot.currentX = msg.pose.pose.position.x;
            robot.currentY = msg.pose.pose.position.y;
            robot.currentZ = msg.pose.pose.position.z;
            qMessage = msg.pose.pose.orientation;
        });
    }

    private void Awake()
    {
        robot = new Robot();
        rosConnection = ROSConnection.GetOrCreateInstance();
        
        var rosSender = FindObjectOfType<ROSSender>();
        rosSender.rosConnection = rosConnection;

        var cameraController = FindObjectOfType<CameraController>();
        cameraController.UIDocument = UIDocument;

        var uiController = FindObjectOfType<UIController>();
        uiController.SetCameraController(cameraController);
        uiController.UIDocument = UIDocument;
        
        
        var connectionController = FindObjectOfType<ConnectionController>();
        connectionController.rosConnection = rosConnection;

        uiController.connectionController = connectionController;
        
        // Give every controller a reference to the robot (Dependency Injection)
        rosSender.robot = robot;
        cameraController.robot = robot;
        uiController.robot = robot;
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRobotPosition();
        UpdateRobotRotation();
        robot3DModel.transform.localPosition = robot.currentPos;
        robot3DModel.transform.rotation = robot.currentRot;
    }

    private void UpdateRobotRotation()
    {
        Quaternion quad = new Quaternion((float) qMessage.x, (float) qMessage.y,(float) qMessage.z, (float)qMessage.w);
        Vector3 angles = quad.eulerAngles;
        angles.x = 0;
        angles.y = -angles.z;
        angles.z = 0;
        Quaternion robotAngle = new Quaternion();
        robotAngle.eulerAngles = angles;
        robot.currentRot = robotAngle;
    }

    private void UpdateRobotPosition()
    {
        //robot.currentPos = new Vector3((float) - pMessage.y, (float) pMessage.z, (float) pMessage.x);
        robot.currentPos = new Vector3((float)- robot.currentY,(float) robot.currentZ, (float) robot.currentX);
    }
}
