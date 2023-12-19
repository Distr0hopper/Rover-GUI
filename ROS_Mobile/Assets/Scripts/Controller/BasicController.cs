using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using myUIController;
using RosMessageTypes.Nav;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UIElements;

public class BasicController : MonoBehaviour
{
    [SerializeField] public string followTargetName = "base";
    [SerializeField] private UIDocument UIDocument;
    private GameObject targetObject;
    private Robot robot;
    private ROSConnection m_Ros {get; set;}
    
    
    void Start()
    {
        targetObject = null;
        // Subscribe to the current pose of the robot
        m_Ros.Subscribe<OdometryMsg>("cur_pose", msg => {
            robot.currentX = msg.pose.pose.position.x;
            robot.currentY = msg.pose.pose.position.y;
        });
    }

    private void Awake()
    {
        robot = new Robot();
        m_Ros = ROSConnection.GetOrCreateInstance();
        // Give every controller a reference to the robot (Dependency Injection)
        var rosSender = FindObjectOfType<ROSSender>();
        rosSender.robot = robot;
        rosSender.m_Ros = m_Ros;

        var cameraController = FindObjectOfType<CameraController>();
        cameraController.robot = robot;
        cameraController.UIDocument = UIDocument;

        var uiController = FindObjectOfType<UIController>();
        uiController.SetRobot(robot);
        uiController.SetCameraController(cameraController);
        uiController.UIDocument = UIDocument;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRobotPos();
    }

    private void UpdateRobotPos()
    {
        if (targetObject == null)
        {
            // Attempt to find the "base" GameObject by name
            targetObject = GameObject.Find(followTargetName);
        }

        else
        {
            robot.currentPos = targetObject.transform.position;
        }
    }
}
