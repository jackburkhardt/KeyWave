using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureAutoResize : MonoBehaviour
{
    private RenderTexture rt;
    private Camera cam;
    public static RenderTexture rtInstance;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        rt = cam.targetTexture;
    }
    
    // Start is called before the first frame update
    public void LateUpdate()
    {
        var mainCam = Camera.main;
        if ( !rt || rt.width != (int)mainCam.pixelWidth || rt.height != (int)mainCam.pixelHeight )
        {
            rt.Release();
            rt.width = (int)mainCam.pixelWidth;
            rt.height = (int)mainCam.pixelHeight;
            rt.Create();
        }
        
        rtInstance = rt;
    }
}
