using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class myPointCloud2DefaultVisualizer : DrawingVisualizerWithSettings<PointCloud2Msg, myPointCloud2VisualizerSettings>
{
    public override string DefaultScriptableObjectPath => "Assets/Resources/myPointCloud2VisualizerSettings";
}