using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
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
}