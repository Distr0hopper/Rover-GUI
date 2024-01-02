using UnityEngine;

namespace Model
{
   
    public class Robot
    {
        
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
        
        public int Duration { get; set; } = 5;

        public enum DIRECTIONS
        {
            stop = 0,
            forward = 1,
            backward = -1,
            left = 5,
            right = 6
        }
        
        public double OrientationX { get; set; }

        public double OrientationY { get; set; }
    
        public double CurrentX;
        public double CurrentY;
        public double CurrentZ;



        public DIRECTIONS Direction { get; set; }
        public Vector3 GoalInWorldPos { get; private set; }
        
        public Vector3 CurrentPos { get; set; }
        public Quaternion CurrentRot { get; set; }

        public void IncrementSpeed()
        {
            if (Duration < 8) Duration++;
        }

        public void DecrementSpeed()
        {
            if (Duration > 1) Duration--;
        }

        public void SetGoalInWorldPos(Vector3 position)
        {
            GoalInWorldPos = position;
        }

        public Vector3 GetGoalInWorldPos()
        {
            return GoalInWorldPos;
        }

    }
}
