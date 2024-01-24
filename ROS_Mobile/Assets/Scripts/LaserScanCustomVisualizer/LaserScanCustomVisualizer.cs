using System;
using RosMessageTypes.Sensor;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class LaserScanCustomVisualizer : DrawingVisualizerWithSettings<LaserScanMsg, LaserScanCustomVisualizerSettings>
{
    public override string DefaultScriptableObjectPath => "VisualizerSettings/LaserScanVisualizerSettings";
}