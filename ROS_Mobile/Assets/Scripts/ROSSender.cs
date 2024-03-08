using System.Collections;
using Utils;
using Model;
using myUIController;
using RosMessageTypes.BuiltinInterfaces;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using UnityEngine.Serialization;
using RosMessageTypes.ROSMobile;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class ROSSender : MonoBehaviour
{
    [FormerlySerializedAs("move_TopicName")] [FormerlySerializedAs("m_TopicName")] [SerializeField]
    private string point_TopicName = "/move_base_simple/goal";
    
    private string lars_manualSteerTopicName = "/man_control";
    private string charlie_manualSteerTopicName = "/manuellController";
    
    // ROS Connector
    public ROSConnection rosConnection { private get; set; }
    void Start()
    {
        // Register publishers for sending point and move commands
        rosConnection.RegisterPublisher<PoseStampedMsg>(point_TopicName);
        rosConnection.RegisterPublisher<Move_commandMsg>(lars_manualSteerTopicName);
        rosConnection.RegisterPublisher<StringMsg>(charlie_manualSteerTopicName);
        rosConnection.RegisterPublisher<BoolMsg>("/cheese/triggerImage");
        rosConnection.RegisterPublisher<BoolMsg>("/uwb/startCalib");
        rosConnection.RegisterPublisher<WD_active_failuresMsg>("/WD/active_failures");
        
        
        // Wait for GUI to be clicked
        UIController.OnStartDriving += SendPointToDrive;
        UIController.OnManualSteering += SendManualStearingCommand;
        UIController.RieglScanStarting += SendRieglScanStartingCommand;
        UIController.LaunchUWB += OnLaunchUWBEvent;
        UIController.StartGeoSama += SendGeosamaScanCommand;
        UIController.StartUwbCalibration += SendUwbCalibration;
        
        // Register ROS Service
        rosConnection.RegisterRosService<EmptyRequest, EmptyResponse>("/setSingle");
        rosConnection.RegisterRosService<EmptyRequest, EmptyResponse>("/startMeasuring");
        
        //rosConnection.RegisterRosService<Int32Msg, EmptyResponse>("/WD/lights_out");
        rosConnection.RegisterRosService<TriggerRequest, TriggerResponse>("/trigger_SDS_servo_1");
        rosConnection.RegisterRosService<TriggerRequest, TriggerResponse>("/trigger_SDS_servo_2");
    }
    
    /*
     * Send the clicked point to the ROS network
     */
    private void SendPointToDrive()
    {
        // Get the coordinates where it wants to drive from Robot Model
        Vector3 worldCoordinates = Robot.Instance.GetGoalInWorldPos();
        var vector3Message = new PointMsg
        {
            x = worldCoordinates.x, // Extract the X coordinate from the GameObject
            y = worldCoordinates.y, // Extract the Y coordinate from the GameObject
            z = worldCoordinates.z // Extract the Z coordinate from the GameObject
        };
        
        // Convert the Vector3 to a ROS message (geometry_message::PoseStamped), end orientation is the orientation while driving to the goal
        PoseStampedMsg messageToRos = ROSUtils.pointToPoseMsg(vector3Message);
        if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
        {
            Lars.Instance.OrientationX = messageToRos.pose.position.x;
            Lars.Instance.OrientationY = messageToRos.pose.position.y;
        }
        else
        {
            Charlie.Instance.OrientationX = messageToRos.pose.position.x;
            Charlie.Instance.OrientationY = messageToRos.pose.position.y;
        }
        

        Debug.Log(messageToRos);
        // Publish the message to the ROS network
        rosConnection.Publish(point_TopicName, messageToRos);
        //rosConnection.SendServiceMessage("/WD/stop_led", new EmptyMsg());
    }
    
    /*
     * Send the manual steering command to the ROS network and update the orientation of the robot
     */
    private void SendManualStearingCommand()
    {
        if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
        {
            SendSteerInfoToLars();
        }
        else
        {
            SendSteerInfoToCharlie();
        }
        Robot.Instance.Angle = Robot.Instance.tempAngle;
    }

    public async void LaunchUWB(Charlie.UWBTRIGGER uwbtrigger)
    {
        if (uwbtrigger == Charlie.UWBTRIGGER.trigger1 || uwbtrigger == Charlie.UWBTRIGGER.trigger3)
        {
            await rosConnection.SendServiceMessage<TriggerResponse>("/trigger_SDS_servo_1",new TriggerRequest());
        } else if (uwbtrigger == Charlie.UWBTRIGGER.trigger2 || uwbtrigger == Charlie.UWBTRIGGER.trigger4)
        {
            await rosConnection.SendServiceMessage<TriggerResponse>("/trigger_SDS_servo_2", new TriggerRequest());
        }
    }
    
    private void OnLaunchUWBEvent()
    {
        LaunchUWB(Charlie.Instance.UwbTrigger);
    }

    private void SendUwbCalibration()
    {
        rosConnection.Publish("/uwb/startCalib", new BoolMsg(true));
    }


    private async void SendRieglScanStartingCommand()
    {
        // Make a service call to start the Riegl scan
        await rosConnection.SendServiceMessage<EmptyResponse>("/setSingle",new EmptyRequest());
        await rosConnection.SendServiceMessage<EmptyResponse>("/startMeasuring",new EmptyRequest());
    }

    private IEnumerator waitForSeconds(int time)
    {
        yield return new WaitForSeconds(time);
    }
    private async void SendWatchdogCommand()
    {
        // Wait for 1 second
        StartCoroutine(waitForSeconds(1));
        
        // Make a service call to start the Riegl scan
        await rosConnection.SendServiceMessage<EmptyResponse>("/WD/lights_out",new UInt32Msg(100));
    }

    private void SendSteerInfoToCharlie()
    {
        StringMsg moveCommandMsg = new StringMsg();
        switch (Robot.Instance.Direction)
        {
            case Robot.DIRECTIONS.stop:
                moveCommandMsg.data = "stop";
                Debug.Log("Command send: " + Robot.Instance.Direction);
                break;
            case Robot.DIRECTIONS.forward:
                moveCommandMsg.data = "fwd " + Robot.Instance.Distance;
                Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Distance + " meters");
                break;
            case Robot.DIRECTIONS.backward:
                moveCommandMsg.data = "rwd " + Robot.Instance.Distance;
                Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Distance + " meters");
                break;
            case Robot.DIRECTIONS.left:
                moveCommandMsg.data = "ccw " + Robot.Instance.Angle;
                Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Angle + " degree");
                break;
            case Robot.DIRECTIONS.right:
                moveCommandMsg.data = "cw " + Robot.Instance.Angle;
                Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Angle + " degree");
                break;
        }
        
        rosConnection.Publish(charlie_manualSteerTopicName, moveCommandMsg);
        
        Charlie.Instance.OrientationX = Charlie.Instance.CurrentX;
        Charlie.Instance.OrientationY = Charlie.Instance.CurrentY;
        
        //rosConnection.Publish("/manuellController", string (msg.data);
        //rosConnection.Publish("/manuellController", new StringMsg("ccw 5"));
        // sending at start: rosConnection.Publish("/manuellController", new StringMsg("scale 0.5"));
    }
    

    private void SendSteerInfoToLars()
    {
        Move_commandMsg moveCommandMsg = new Move_commandMsg();
        // If Stop button is clicked, send stop command
        if (Robot.Instance.Direction == 0)
        {
            moveCommandMsg.stop = true;
        } else moveCommandMsg.stop = false;
      
        moveCommandMsg.setSpeed = false;
        
        moveCommandMsg.direction = (sbyte)Robot.Instance.Direction;
        
        if (Robot.Instance.ManualMode == Robot.MANUALMODE.drive)
        {
            moveCommandMsg.value = Robot.Instance.Distance;
            Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Distance + " meters");
        
        } else if (Robot.Instance.ManualMode == Robot.MANUALMODE.rotate)
        {
            moveCommandMsg.value = Robot.Instance.Angle;
            Debug.Log("Direction: " + Robot.Instance.Direction + " for " + Robot.Instance.Angle + " degree");
        }
        rosConnection.Publish(lars_manualSteerTopicName, moveCommandMsg);
        Lars.Instance.OrientationX = Lars.Instance.CurrentX;
        Lars.Instance.OrientationY = Lars.Instance.CurrentY;
    }

    private void SendGeosamaScanCommand()
    {
        Debug.Log("Scanned");
        rosConnection.Publish("/cheese/triggerImage", new BoolMsg(true));
    }
}
