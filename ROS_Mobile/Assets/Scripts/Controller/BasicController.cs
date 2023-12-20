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
    [SerializeField] public string followTargetName = "base_link";
    [SerializeField] private UIDocument UIDocument;
    [SerializeField] public GameObject robot3DModel;
    private GameObject targetObject;
    private Robot robot;
    private ROSConnection m_Ros {get; set;}
    
    QuaternionMsg qMessage = new QuaternionMsg();
    
    void Start()
    {
        targetObject = null;
        // Subscribe to the current pose of the robot
        m_Ros.Subscribe<OdometryMsg>("cur_pose", msg => {
            robot.currentX = msg.pose.pose.position.x;
            robot.currentY = msg.pose.pose.position.y;
            robot.currentZ = msg.pose.pose.position.z;
            qMessage = msg.pose.pose.orientation;
        });
    }

    private void Awake()
    {
        robot = new Robot();
        m_Ros = ROSConnection.GetOrCreateInstance();
        var rosSender = FindObjectOfType<ROSSender>();
        rosSender.m_Ros = m_Ros;

        var cameraController = FindObjectOfType<CameraController>();
        cameraController.UIDocument = UIDocument;

        var uiController = FindObjectOfType<UIController>();
        // Give every controller a reference to the robot (Dependency Injection)
        rosSender.robot = robot;
        cameraController.robot = robot;
        uiController.SetRobot(robot);
        
        uiController.SetCameraController(cameraController);
        uiController.UIDocument = UIDocument;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        robot.currentPos = new Vector3((float)- robot.currentY,(float) robot.currentZ, (float) robot.currentX);
        robot3DModel.transform.localPosition = robot.currentPos;
        Quaternion quad = new Quaternion((float) qMessage.x, (float) qMessage.y,(float) qMessage.z, (float)qMessage.w);
        Vector3 angles = quad.eulerAngles;
        angles.x = 0;
        angles.y = -angles.z;
        angles.z = 0;
        Quaternion robotAngle = new Quaternion();
        robotAngle.eulerAngles = angles;
    }

    private void FindObject()
    {
        if (targetObject == null)
        {
            // Attempt to find the "base" GameObject by name
            targetObject = GameObject.Find(followTargetName);
        }
    }
}
