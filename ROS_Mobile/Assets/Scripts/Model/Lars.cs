using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lars
{
    private static Lars _instance;
    
    //Singleton pattern 
    public static Lars Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Lars();
            }
            return _instance;
        } 
    }
    
    public double CurrentX;
    public double CurrentY;
    public double CurrentZ;

  
    
    public double OrientationX { get; set; }

    public double OrientationY { get; set; }
        
    public Vector3 CurrentPos { get; set; }
    public Quaternion CurrentRot { get; set; }
}
