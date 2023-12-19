using Utils;
using Model;
using myUIController;
using RosMessageTypes.BuiltinInterfaces;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using UnityEngine.Serialization;
using RosMessageTypes.ROSMobile;
public class ROSSender : MonoBehaviour
{
    [FormerlySerializedAs("move_TopicName")] [FormerlySerializedAs("m_TopicName")] [SerializeField]
    private string point_TopicName = "/move_base_simple/goal";

    private string stear_TopicName = "/man_control";

    public Robot robot { private get; set; }
    
    // ROS Connector
    public ROSConnection m_Ros { private get; set; }
    void Start()
    {
        // Register publishers for sending point and move commands
        m_Ros.RegisterPublisher<PoseStampedMsg>(point_TopicName);
        m_Ros.RegisterPublisher<Move_commandMsg>(stear_TopicName);

        // Wait for GUI to be clicked
        UIController.OnStartDriving += sendPointToDrive;
        UIController.OnManualSteering += sendManualStearingCommand;
    }
    
    private void sendPointToDrive()
    {
        // Get the coordinates from Model
        Vector3 worldCoordinates = robot.getGoalInWorldPos();
        var vector3Message = new PointMsg
        {
            x = worldCoordinates.x, // Extract the X coordinate from the GameObject
            y = worldCoordinates.y, // Extract the Y coordinate from the GameObject
            z = worldCoordinates.z // Extract the Z coordinate from the GameObject
        };
        
        PoseStampedMsg messageToRos = ROSUtils.pointToPoseMsg(vector3Message, robot.orientationX, robot.orientationY);
        robot.orientationX = messageToRos.pose.position.x;
        robot.orientationY = messageToRos.pose.position.y;
        // Vector3 to geometry_message::PoseStamped
        // PoseStamped hat Header (Std_msgs) - Pose (geometry_msgs::Pose)
        Debug.Log(messageToRos);
        // Publish the message to the ROS network
        m_Ros.Publish(point_TopicName, messageToRos);
    }
    
    private void sendManualStearingCommand()
    {
        Debug.Log("ROSSender: " + robot.Direction);
        Move_commandMsg moveCommandMsg = new Move_commandMsg();
        moveCommandMsg.setSpeed = false;
        if (robot.Direction == 0)
        {
            moveCommandMsg.stop = true;
        } else moveCommandMsg.stop = false;

        Debug.Log("Speed: " + robot.Speed);
        moveCommandMsg.direction = (sbyte)robot.Direction;
        moveCommandMsg.duration = new DurationMsg(robot.Speed);
        m_Ros.Publish(stear_TopicName, moveCommandMsg);
        robot.orientationX = robot.currentX;
        robot.orientationY = robot.currentY;
    }
    
}
