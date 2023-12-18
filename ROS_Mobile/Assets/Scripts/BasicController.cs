using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using myUIController;

public class BasicController : MonoBehaviour
{
    public string followTargetName = "base";
    private GameObject targetObject;
    private Robot robot;
    
    void Start()
    {
        targetObject = null;
    }

    private void Awake()
    {
        robot = new Robot();
        // Give every controller a reference to the robot (Dependency Injection)
        var rosSender = FindObjectOfType<ROSSender>();
        rosSender.SetRobot(robot);

        var cameraController = FindObjectOfType<CameraController>();
        cameraController.SetRobot(robot);

        var uiController = FindObjectOfType<UIController>();
        uiController.SetRobot(robot);
        uiController.SetCameraController(cameraController);
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
