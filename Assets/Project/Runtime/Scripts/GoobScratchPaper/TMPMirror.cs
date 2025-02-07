using NaughtyAttributes;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TMPMirror : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProToMirror;
   
    private TextMeshProUGUI _textMeshPro;
    public bool useFormat;
    public bool mirrorColor;
    
    [ShowIf("useFormat")]
    [ReadOnly] public string surrogate = "{0}";
    [ShowIf("useFormat")]
    public string format;
   
    void Update()
    {
        _textMeshPro ??= GetComponent<TextMeshProUGUI>();
        
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
