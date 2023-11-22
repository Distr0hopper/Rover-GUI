using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class Vector3Sender : MonoBehaviour
{
    [SerializeField]
    string m_TopicName = "/move_base_simple/goal"; 

    [SerializeField]
    GameObject m_Target; // The GameObject from which to extract the Vector3

    // ROS Connector
    ROSConnection m_Ros;

    void Start()
    {
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<PointMsg>(m_TopicName);
    }

    public void Update()
    {
        if (MouseClickCoordinates.GetBoolClicked())
        {
            // Create a Point message to represent a Vector3
            var vector3Message = new PointMsg
            {
                x = m_Target.transform.position.x, // Extract the X coordinate from the GameObject
                y = m_Target.transform.position.y, // Extract the Y coordinate from the GameObject
                z = m_Target.transform.position.z  // Extract the Z coordinate from the GameObject
            };
            // Vector3 to geometry_message::PoseStamped
            //PoseStamped hat Header (Std_msgs) - Pose (geometry_msgs::Pose)


            //Log the x coordinate of the Vector3
            //UnityEngine.Debug.Log("Vector3 x: " + vector3Message.x);

            m_Ros.Publish(m_TopicName, vector3Message);

            // Reset the boolClicked variable
            MouseClickCoordinates.SetBoolClicked(false);
        }
    }
}
