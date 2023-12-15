using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
public class PointCloud2CustomVisualizer :  DrawingVisualizerWithSettings<PointCloud2Msg, PointCloud2CustomVisualizerSettings>

{
    public override string DefaultScriptableObjectPath => "VisualizerSettings/PointCloud2VisualizerSettings";
}
