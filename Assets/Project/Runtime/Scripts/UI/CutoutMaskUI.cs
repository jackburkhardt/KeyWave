using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CutoutMaskUI : Image
{
    // Start is called before the first frame update
    public override Material materialForRendering
    {
        get
        {
            var material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
