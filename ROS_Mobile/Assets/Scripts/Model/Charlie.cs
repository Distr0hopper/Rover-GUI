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
    
    public double CurrentX;
    public double CurrentY;
    public double CurrentZ;
    
    public double theta;
    
    public double OrientationX { get; set; }

    public double OrientationY { get; set; }
        
    public Vector3 CurrentPos { get; set; }
    public Quaternion CurrentRot { get; set; }
}
