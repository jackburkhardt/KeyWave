using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class TMPMirror : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI textMeshProToMirror;
   
    private TMPro.TextMeshProUGUI _textMeshPro;
    public bool useFormat;
    public bool mirrorColor;
    
    [ShowIf("useFormat")]
    [ReadOnly] public string surrogate = "{0}";
    [ShowIf("useFormat")]
    public string format;
   
    void Update()
    {
        _textMeshPro ??= GetComponent<TMPro.TextMeshProUGUI>();
        
        if (textMeshProToMirror == null || textMeshProToMirror == null) return;
        
        if (useFormat)
        {
            _textMeshPro.text = format.Replace(surrogate, textMeshProToMirror.text);
            return;
        }
        
        _textMeshPro.text = textMeshProToMirror.text;
        
        if (mirrorColor)
        {
            _textMeshPro.color = textMeshProToMirror.color;
        }
    }
}
