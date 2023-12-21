using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    [SerializeField]
    private string charlie_ip = "192.168.0.122";
    [SerializeField]
    private string lars_ip = "192.168.0.52";

    public ROSConnection rosConnection { private get; set; }

    void Start()
    {
        rosConnection.RosIPAddress = charlie_ip;
    }

    public void changeRobotIP()
    {
        switch (BasicController.ActiveRobot)
        {
            case BasicController.ACTIVEROBOT.Charlie:
                setCharlieIP();
                break;
            case BasicController.ACTIVEROBOT.Lars:
                setLarsIP();
                break;
        }
    }
    
    private void setCharlieIP()
    {
        rosConnection.RosIPAddress = charlie_ip;
    }
    
    private void setLarsIP()
    {
        rosConnection.RosIPAddress = lars_ip;
    }
    
}
