using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Visualizations;
using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    [SerializeField]
    private string charlie_ip = "192.168.0.227";
    [SerializeField]
    private string lars_ip = "192.168.0.52";

    [SerializeField] private PointCloud2CustomVisualizerSettings pointClouds;
    public ROSConnection rosConnection { private get; set; }
    
    private bool _hasConnection;
    
    // Define method for connection status changed event
    public delegate void ConnectionStatusChangedHandler(bool isConnected);
    
    // Event for connection status changed
    public event ConnectionStatusChangedHandler OnConnectionStatusChanged;
    

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

    public void ChangeRobotIP()
    {
        switch (Robot.Instance.ActiveRobot)
        {
            case Robot.ACTIVEROBOT.Charlie:
                rosConnection.Disconnect();
                SetCharlieIP();
                rosConnection.Connect();
                break;
            case Robot.ACTIVEROBOT.Lars:
                rosConnection.Disconnect();
                SetLarsIP();
                rosConnection.Connect();
                pointClouds.DestroyAllPointclouds();
                break;
        }
    }

    private void ClearAllPointclouds()
    {
        foreach (var pointCloud in PointCloudDrawing.gameObjects)
        {
            pointCloud.SetActive(false);
        }
    }
    
    private void SetCharlieIP()
    {
        rosConnection.RosIPAddress = charlie_ip;
    }
    
    private void SetLarsIP()
    {
        rosConnection.RosIPAddress = lars_ip;
    }
    
}
