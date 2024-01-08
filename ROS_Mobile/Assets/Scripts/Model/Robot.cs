using UnityEngine;

namespace Model
{
   
    public class Robot
    {
        
        private static Robot _instance;
        
        // Singleton Pattern for  RobotModel
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
        
        #region ENUMS

        // Current Mode of the robot
        public enum OperationMode
        {
            autoDrive,
            manualDrive,
            uwbMission,
            geoSamaMission
        }

        public OperationMode _operationMode = OperationMode.autoDrive;
        public enum DIRECTIONS
        {
            stop = 0,
            forward = 1,
            backward = -1,
            left = 5,
            right = 6
        }
        public DIRECTIONS Direction { get; set; }

        public enum UWBTRIGGER
        {
            trigger1 = 1,
            trigger2 = 2,
            trigger3 = 3,
            trigger4 = 4,
            noTrigger = 0
        }

        public UWBTRIGGER UwbTrigger { get; set; } = UWBTRIGGER.noTrigger;
        
        public enum ACTIVEROBOT
        {
            Charlie = 0,
            Lars = 1
        }

        public ACTIVEROBOT ActiveRobot { get; set; } = ACTIVEROBOT.Charlie;

        #endregion
        
        // 3D Model of the robot in the scene
        public GameObject Robot3DModel { get; set; }
        public int Duration { get; set; } = 5;

        #region World Coordinates
        public double OrientationX { get; set; }

        public double OrientationY { get; set; }
    
        public double CurrentX;
        public double CurrentY;
        public double CurrentZ;
        
        public Vector3 GoalInWorldPos { get; private set; }
        
        public Vector3 CurrentPos { get; set; }
        public Quaternion CurrentRot { get; set; }
        
        #endregion
        
        /*
         * TODO:
         * List that contains all triggers and removes them if they are triggered or
         * bools that are true if the trigger is triggered
         */
      
        


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

        public void HideModel()
        {
            Robot3DModel.SetActive(false);
        }
        
        public void ShowModel()
        {
            Robot3DModel.SetActive(true);
        }

        public bool IsModelActive()
        {
            return Robot3DModel.activeSelf;
        }

    }
}
