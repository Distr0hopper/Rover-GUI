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
    public ROSConnection rosConnection { private get; set; }
    void Start()
    {
        // Register publishers for sending point and move commands
        rosConnection.RegisterPublisher<PoseStampedMsg>(point_TopicName);
        rosConnection.RegisterPublisher<Move_commandMsg>(stear_TopicName);

        // Wait for GUI to be clicked
        UIController.OnStartDriving += SendPointToDrive;
        UIController.OnManualSteering += SendManualStearingCommand;
    }
    
    /*
     * Send the clicked point to the ROS network
     */
    private void SendPointToDrive()
    {
        // Get the coordinates where it wants to drive from Robot Model
        Vector3 worldCoordinates = robot.getGoalInWorldPos();
        var vector3Message = new PointMsg
        {
            x = worldCoordinates.x, // Extract the X coordinate from the GameObject
            y = worldCoordinates.y, // Extract the Y coordinate from the GameObject
            z = worldCoordinates.z // Extract the Z coordinate from the GameObject
        };
        
        // Convert the Vector3 to a ROS message (geometry_message::PoseStamped), end orientation is the orientation while driving to the goal
        PoseStampedMsg messageToRos = ROSUtils.pointToPoseMsg(vector3Message, robot);
        robot.orientationX = messageToRos.pose.position.x;
        robot.orientationY = messageToRos.pose.position.y;

        Debug.Log(messageToRos);
        // Publish the message to the ROS network
        rosConnection.Publish(point_TopicName, messageToRos);
    }
    
    /*
     * Send the manual steering command to the ROS network and update the orientation of the robot
     */
    private void SendManualStearingCommand()
    {
        Debug.Log("Direction: " + robot.Direction);
        Move_commandMsg moveCommandMsg = new Move_commandMsg();
        moveCommandMsg.setSpeed = false;
        if (robot.Direction == 0)
        {
            moveCommandMsg.stop = true;
        } else moveCommandMsg.stop = false;
        
        moveCommandMsg.direction = (sbyte)robot.Direction;
        moveCommandMsg.duration = new DurationMsg(robot.Duration);
        rosConnection.Publish(stear_TopicName, moveCommandMsg);
        robot.orientationX = robot.currentX;
        robot.orientationY = robot.currentY;
    }
    
}
