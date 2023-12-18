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
        public Vector3 goalInWorldPos { get; private set; }
        
        public Vector3 currentPos { get; set; }

        public void incrementSpeed()
        {
            if (Speed < 8) Speed++;
        }

        public void decrementSpeed()
        {
            if (Speed > 1) Speed--;
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
