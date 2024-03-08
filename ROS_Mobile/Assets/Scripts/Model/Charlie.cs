using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charlie
{
    private static Charlie _instance;
    
    //Singleton pattern 
    public static Charlie Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Charlie();
            }
            return _instance;
        } 
    }
    
    public enum UWBTRIGGER
    {
        trigger1 = 1,
        trigger2 = 2,
        trigger3 = 3,
        trigger4 = 4,
        noTrigger = 0
    }

    public UWBTRIGGER UwbTrigger { get; set; } = UWBTRIGGER.noTrigger;
    
    public double CurrentX;
    public double CurrentY;
    public double CurrentZ;
    
    public double theta;
    
    public double OrientationX { get; set; }

    public double OrientationY { get; set; }
        
    public Vector3 CurrentPos { get; set; }
    public Quaternion CurrentRot { get; set; }

    public bool UwbCalibDone { get; set; } = false;
}
