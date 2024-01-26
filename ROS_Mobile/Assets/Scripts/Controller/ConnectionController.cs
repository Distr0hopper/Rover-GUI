using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using myUIController;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Visualizations;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionController : MonoBehaviour
{
    [SerializeField]
    private string charlie_ip = "192.168.0.227";
    [SerializeField]
    private string lars_ip = "192.168.0.52";

    [SerializeField] private PointCloud2CustomVisualizerSettings pointClouds;
    [SerializeField] private LaserScanCustomVisualizerSettings laserScan;

    public UIController uiController;
    public ROSConnection rosConnection { get; set; }
    
    private bool _hasConnection;
    private float connectionCheckDelay = 1.0f; // Delay in seconds
    private float lastConnectionCheckTime;
    
    
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
        // Check connection status at intervals defined by connectionCheckDelay
        if (Time.time - lastConnectionCheckTime > connectionCheckDelay)
        {
            lastConnectionCheckTime = Time.time;
            CheckConnection();
        }
    }
    
    private void CheckConnection()
    {
        // Logic to check the actual connection status
        bool currentConnectionStatus = !rosConnection.HasConnectionError;
        HasConnection = currentConnectionStatus;
    }

    public void ChangeRobotIP()
    {
        switch (Robot.Instance.ActiveRobot)
        {
            case Robot.ACTIVEROBOT.Charlie:
                rosConnection.Disconnect();
                SetCharlieIP();
                rosConnection.Connect();
                laserScan.DestroyDrawing();
                pointClouds.ClearPointcloudList(); //Clear the list, since if the robot switch was too fast, the list may notq be empty 
                /*
                rosConnection.Unsubscribe("/camera/fisheye2/image_raw/compressed");
                rosConnection.Subscribe<CompressedImageMsg>("/camera/fisheye1/image_raw/compressed", msg =>
                {
                    uiController.RenderRealsenseCamera(msg);
                });
                rosConnection.RegisterPublisher<CompressedImageMsg>("/camera/fisheye1/image_raw/compressed");
                */
                break;
            case Robot.ACTIVEROBOT.Lars:
                rosConnection.Disconnect();
                SetLarsIP();
                rosConnection.Connect();
                pointClouds.DestroyDrawing();
                /*
                rosConnection.Unsubscribe("/camera/fisheye1/image_raw/compressed");
                rosConnection.Subscribe<CompressedImageMsg>("/camera/fisheye2/image_raw/compressed", msg =>
                {
                    uiController.RenderGeoSamaCamera(msg);
                });
                rosConnection.RegisterPublisher<CompressedImageMsg>("/camera/fisheye2/image_raw/compressed");
                */
                break;
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
