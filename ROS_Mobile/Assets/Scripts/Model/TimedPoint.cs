using UnityEngine;

namespace Model
{
    public class TimedPoint 
    {
        public Vector3 unityPoint;
        public Color color;
        public float radius;
        public float timestamp;

        public TimedPoint(Vector3 unityPoint, Color color, float radius, float timestamp)
        {
            this.unityPoint = unityPoint;
            this.color = color;
            this.radius = radius;
            this.timestamp = timestamp;
        }
    }
}

