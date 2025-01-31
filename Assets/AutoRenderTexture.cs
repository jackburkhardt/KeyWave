using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoRenderTexture : MonoBehaviour
{
    private void Update()
    {
        GetComponent<RawImage>().texture = RenderTextureAutoResize.rtInstance;
    }
}
