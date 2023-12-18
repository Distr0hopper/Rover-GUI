using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.Visualizations;
using UnityEngine;

namespace Model
{

    class MyPointCloudDrawing
    {
        public float timestamp;
        public PointCloudDrawing pointCloudDrawing;


        public MyPointCloudDrawing(PointCloudDrawing pointCloudDrawing, float timestamp)
        {
            this.pointCloudDrawing = pointCloudDrawing;
            this.timestamp = timestamp;
        }
        
    }
}