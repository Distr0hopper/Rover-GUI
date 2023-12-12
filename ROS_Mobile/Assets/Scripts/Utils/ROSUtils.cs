using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using UnityEngine;

namespace Utils
{
public static class ROSUtils  {
    public static PoseStampedMsg pointToPoseMsg(PointMsg unityMessage,double orientationX, double orientationY)
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
        
        // Invert axis because ROS using other Coordinate System
        rosMessage.pose.position = new PointMsg();
        rosMessage.pose.position.x = unityMessage.z;
        rosMessage.pose.position.y = - unityMessage.x;
        rosMessage.pose.position.z = unityMessage.y;

        // Calculate the orientation
        Vector2 orientationToFinish = new Vector2((float)(rosMessage.pose.position.x - orientationX), (float)(rosMessage.pose.position.y - orientationY));
        float angle = Vector2.SignedAngle(Vector2.right, orientationToFinish);
        rosMessage.pose.orientation = new QuaternionMsg
        {
            x = 0,
            y = 0,
            z = Mathf.Sin(angle * Mathf.Deg2Rad / 2),
            w = Mathf.Cos(angle * Mathf.Deg2Rad / 2)
        };
        
        orientationX = rosMessage.pose.position.x;
        orientationY = rosMessage.pose.position.y;
        
        return rosMessage;
    }
   
}
    
}
