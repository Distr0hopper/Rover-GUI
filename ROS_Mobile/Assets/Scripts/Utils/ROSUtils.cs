using Model;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using UnityEngine;

namespace Utils
{
public static class ROSUtils  {
    /*
     * Convert a Vector3 to a ROS message (geometry_message::PoseStamped)
     * The end orientation is the orientation while driving to the goal
     */
    public static PoseStampedMsg pointToPoseMsg(PointMsg unityMessage)
    {
        PoseStampedMsg rosMessage = new PoseStampedMsg();
        //rosMessage.header = new HeaderMsg();
        //rosMessage.pose = new PoseMsg();
        
        // Get the current time since epoch in seconds and nanoseconds for header.stamp
        double stamp_secs = Epoch.epochTime();
        double nanoseconds = Epoch.nanosecSinceStmp(stamp_secs);
        
        // Set the header
        rosMessage.header.seq = 0;
        rosMessage.header.stamp = new TimeMsg((uint)stamp_secs, (uint)nanoseconds);
        rosMessage.header.frame_id = "odom_frame";
        
        rosMessage.pose.position = new PointMsg();
        Vector2 orientationToFinish = new Vector2();
        // Invert axis because ROS using other Coordinate System
        if(Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Lars)
        {
            rosMessage.pose.position.x = unityMessage.z;
            rosMessage.pose.position.y = - unityMessage.x;
            rosMessage.pose.position.z = unityMessage.y;
            
            orientationToFinish = new Vector2((float)(rosMessage.pose.position.x - Lars.Instance.OrientationX), (float)(rosMessage.pose.position.y - Lars.Instance.OrientationY));
            
        } else if (Robot.Instance.ActiveRobot == Robot.ACTIVEROBOT.Charlie)
        {
            rosMessage.pose.position.x = -unityMessage.x;
            rosMessage.pose.position.y = -unityMessage.z;
            rosMessage.pose.position.z = unityMessage.y;
            
            orientationToFinish = new Vector2((float)(rosMessage.pose.position.x - Charlie.Instance.OrientationX), (float)(rosMessage.pose.position.y - Charlie.Instance.OrientationY));
        }

        // Calculate the orientation
       
        float angle = Vector2.SignedAngle(Vector2.right, orientationToFinish);
        rosMessage.pose.orientation = new QuaternionMsg
        {
            x = 0,
            y = 0,
            z = Mathf.Sin(angle * Mathf.Deg2Rad / 2),
            w = Mathf.Cos(angle * Mathf.Deg2Rad / 2)
        };
        return rosMessage;
    }
   
}
    
}
