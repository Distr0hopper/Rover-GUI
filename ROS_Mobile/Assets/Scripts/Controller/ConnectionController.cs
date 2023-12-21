using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    [SerializeField]
    private string charlie_ip = "192.168.56.20";
    [SerializeField]
    private string lars_ip = "192.118.52.31";

    public ROSConnection rosConnection { private get; set; }
    
    // Define method for connection status changed event
    public delegate void ConnectionStatusChangedHandler(bool isConnected);
    // Event for connection status changed
    public event ConnectionStatusChangedHandler OnConnectionStatusChanged;
    
    private bool _hasConnection;

    // Public property for connection status, only this class can set it
    public bool HasConnection
    {
        get => _hasConnection;
        private set
        {
            // Has the connection status changed?
            if (_hasConnection != value)
            {
                _hasConnection = value;
                // Notify all listeners, check if there are any (null check with ?) 
                OnConnectionStatusChanged?.Invoke(_hasConnection);
            }
        }
    }

    void Start()
    {
        rosConnection.RosIPAddress = charlie_ip;
        rosConnection.Connect();
    }

    private void Update()
    {
        HasConnection = !rosConnection.HasConnectionError;
    }

    public void changeRobotIP()
    {
        switch (BasicController.ActiveRobot)
        {
            case BasicController.ACTIVEROBOT.Charlie:
                setCharlieIP();
                rosConnection.Connect();
                break;
            case BasicController.ACTIVEROBOT.Lars:
                setLarsIP();
                rosConnection.Connect();
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