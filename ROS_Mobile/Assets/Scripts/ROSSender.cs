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

    private double orientationX;

    private double orientationY;
    
    private double currentX;
    
    private double currentY;
    
    // ROS Connector
    ROSConnection m_Ros;
    void Start()
    {
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        // Register publishers for sending point and move commands
        m_Ros.RegisterPublisher<PoseStampedMsg>(point_TopicName);
        m_Ros.RegisterPublisher<Move_commandMsg>(stear_TopicName);
        // Subscribe to the current pose of the robot
        m_Ros.Subscribe<OdometryMsg>("cur_pose", msg => {
            currentX = msg.pose.pose.position.x;
            currentY = msg.pose.pose.position.y;
        });
        // Wait for GUI to be clicked
        UIController.OnStartDriving += sendPointToDrive;
        UIController.OnManualSteering += sendManualStearingCommand;
    }
    
    private void sendPointToDrive()
    {
        Debug.Log(Robot.Instance.GetHashCode());
        // Get the coordinates from Model
        Vector3 worldCoordinates = Robot.Instance.getWorldCoordinates();
        var vector3Message = new PointMsg
        {
            x = worldCoordinates.x, // Extract the X coordinate from the GameObject
            y = worldCoordinates.y, // Extract the Y coordinate from the GameObject
            z = worldCoordinates.z // Extract the Z coordinate from the GameObject
        };
        
        PoseStampedMsg messageToRos = ROSUtils.pointToPoseMsg(vector3Message, orientationX, orientationY);
        orientationX = messageToRos.pose.position.x;
        orientationY = messageToRos.pose.position.y;
        // Vector3 to geometry_message::PoseStamped
        // PoseStamped hat Header (Std_msgs) - Pose (geometry_msgs::Pose)
        Debug.Log(messageToRos);
        // Publish the message to the ROS network
        m_Ros.Publish(point_TopicName, messageToRos);
    }
    
    private void sendManualStearingCommand()
    {
        Debug.Log("ROSSender: " + Robot.Instance.Direction);
        Move_commandMsg moveCommandMsg = new Move_commandMsg();
        moveCommandMsg.setSpeed = false;
        if (Robot.Instance.Direction == 0)
        {
            moveCommandMsg.stop = true;
        } else moveCommandMsg.stop = false;

        Debug.Log("Speed: " + Robot.Instance.Speed);
        moveCommandMsg.direction = (sbyte)Robot.Instance.Direction;
        moveCommandMsg.duration = new DurationMsg(Robot.Instance.Speed);
        m_Ros.Publish(stear_TopicName, moveCommandMsg);
        orientationX = currentX;
        orientationY = currentY;
    }
}
