using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]

public class TMPAutoScale : MonoBehaviour
{
    public RectTransform fitTo;
    
    public TextMeshProUGUI text;

    public float minFontSize;
    public float maxFontSize;

    private void Update()
    {
        throw new NotImplementedException();
    }
}
