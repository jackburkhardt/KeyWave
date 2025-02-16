using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoRenderTexture : MonoBehaviour
{
    private RawImage rawImage;
    private void OnEnable()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        rawImage.texture = RenderTextureAutoResize.rtInstance;
    }
}
