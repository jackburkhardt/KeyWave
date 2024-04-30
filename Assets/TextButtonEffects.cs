using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TextButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    private Button button => GetComponent<Button>();
    
   
    public void OnPointerEnter(PointerEventData eventData)
    {
      //  Debug.Log("Pointer Enter");
      //  _text.color = button.colors.highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _text.color = _defaultColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _text.color = button.colors.pressedColor;
    }

    [SerializeField] private UITextField _text;
    
    private Color _defaultColor;
    
    private void Awake()
    {
        _defaultColor = _text.color;
    }
    

}
