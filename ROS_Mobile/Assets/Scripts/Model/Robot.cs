using UnityEngine;

namespace Model
{
   
    public class Robot
    {
        /*
        // Singleton Pattern for  RobotModel
        private static Robot _instance;
        public static Robot Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Robot();
                }
                return _instance;
            }
        }
        */
        public int Duration { get; private set; } = 5;

        public enum DIRECTIONS
        {
            stop = 0,
            forward = 1,
            backward = -1,
            left = 5,
            right = 6
        }
        
        public double orientationX { get; set; }

        public double orientationY { get; set; }
    
        public double currentX;
        public double currentY;
        public double currentZ;



        public DIRECTIONS Direction { get; set; }
        public Vector3 goalInWorldPos { get; private set; }
        
        public Vector3 currentPos { get; set; }
        public Quaternion currentRot { get; set; }

        public void incrementSpeed()
        {
            if (Duration < 8) Duration++;
        }

        public void decrementSpeed()
        {
            if (Duration > 1) Duration--;
        }

        public void setGoalInWorldPos(Vector3 position)
        {
            goalInWorldPos = position;
        }

        public Vector3 getGoalInWorldPos()
        {
            return goalInWorldPos;
        }

    }
}
