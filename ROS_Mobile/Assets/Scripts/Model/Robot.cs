using System;
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
            geoSamaMission,
            rieglScan
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
        
        public enum ACTIVEROBOT
        {
            Charlie = 0,
            Lars = 1
        }

        public float tempAngle;

        public ACTIVEROBOT ActiveRobot { get; set; } = ACTIVEROBOT.Charlie;

        public enum MANUALMODE
        {
            rotate = 0,
            drive = 1
        }
        
        public MANUALMODE ManualMode { get; set; } = MANUALMODE.drive;

        #endregion
        
        // 3D Model of the robot in the scene
        public GameObject Robot3DModel { get; set; }
        //public int Duration { get; set; } = 5;
        
        public float Distance { get; set; } = 0f;
        public float Angle { get; set; } = 0f;

        #region World Coordinates
        
        public Vector3 GoalInWorldPos { get; private set; }
        
        public Vector3 CurrentPos { get; set; }
        public Quaternion CurrentRot { get; set; }
        
        #endregion
        
        /*
         * TODO:
         * List that contains all triggers and removes them if they are triggered or
         * bools that are true if the trigger is triggered
         */
      
        


        public void IncrementDistance(float step = 0.5f)
        {
            Distance+=step;
            if (Distance >= 8) Distance = 8;
            Distance = Mathf.Round(Distance * 10f) / 10f;
        }

        public void DecrementDistance(float step = 0.5f)
        {
            Distance-=step;
            if (Distance <= 0) Distance = 0;
            Distance = Mathf.Round(Distance * 10f) / 10f;
        }
        
        public void IncrementAngle(float step = 5f)
        {
            Angle += step;
            if (Angle > 360) Angle = 360;
        }
        
        public void DecrementAngle(float step = 5f)
        {
            Angle -= step;
            if (Angle <= 0) Angle = 0;
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
