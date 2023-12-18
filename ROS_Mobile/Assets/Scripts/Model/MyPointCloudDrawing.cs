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
        private float decayTime = 0.0f;

        Coroutine m_decayTimeCoroutine = null;

        public MyPointCloudDrawing(PointCloudDrawing pointCloudDrawing, float timestamp, float decayTime)
        {
            this.pointCloudDrawing = pointCloudDrawing;
            this.timestamp = timestamp;
            this.decayTime = decayTime;
        }



    }
}