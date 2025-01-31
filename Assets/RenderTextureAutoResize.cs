using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenderTextureAutoResize : MonoBehaviour
{
    private RenderTexture rt;
    public static RenderTexture rtInstance;
    

    // Start is called before the first frame update
    public void Update()
    {
        rt = GetComponent<Camera>().targetTexture;
        
        if ( !rt || rt.width != (int)Camera.main.pixelWidth || rt.height != (int)Camera.main.pixelHeight )
        {
            if (rt)
            {
                if (RenderTexture.active == rt)
                    RenderTexture.active = null;
                RenderTexture.DestroyImmediate(rt);
                rt = null;
            }
	
            GetComponent<Camera>().ResetAspect();
            
            rt = new RenderTexture((int)Camera.main.pixelWidth, (int)Camera.main.pixelHeight, 16);
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.filterMode = FilterMode.Point;
            rt.hideFlags = HideFlags.DontSave; 
            rt.isPowerOfTwo = false;
            rt.Create();
			
            RenderTexture.active = rt;
            GL.Clear(true, true, new Color(0,0,0,0));
            RenderTexture.active = null;
			
            GetComponent<Camera>().targetTexture = rt;
        }
        
        rtInstance = rt;
    }
}
