using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
   
    public class RobotModel
    {
        // Singleton of RobotModel
        private static RobotModel _instance;
        public static RobotModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RobotModel();
                }
                return _instance;
            }
        }
        public int Speed { get; private set; } = 5;

        public enum DIRECTIONS
        {
            stop = 0,
            forward = 1,
            backward = -1,
            left = 5,
            right = 6
        }

        public DIRECTIONS Direction { get; set; }
        public Vector3 WorldPosition { get; private set; }

        public void incrementSpeed()
        {
            if (Speed < 8) Speed++;
        }

        public void decrementSpeed()
        {
            if (Speed > 1) Speed--;
        }

        public void SetWorldPosition(Vector3 position)
        {
            WorldPosition = position;
        }

        public Vector3 getWorldCoordinates()
        {
            return WorldPosition;
        }

    }
}
