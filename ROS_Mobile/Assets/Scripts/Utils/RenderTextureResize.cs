using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
public class RenderTextureResize
{
    public static RenderTexture Resize(RenderTexture rt, float width, float height)
    {
        rt.Release();
        rt.width = (int) width;
        rt.height = (int) height;
        //rt.Create();
        return rt;
    }
    
    
}
    
}
