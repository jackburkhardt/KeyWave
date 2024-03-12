using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoPanelButton : MonoBehaviour
{
    private List<CircularUIButton> watchEdgeButtons;
    
    private bool _isMouseOver;

    private RectTransform _rectTransform;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        watchEdgeButtons = FindObjectsOfType<CircularUIButton>().ToList();
    }

    private void OnDisable()
    {
        foreach (var button in watchEdgeButtons)
        {
            button.GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        var pointer = new PointerEventData(EventSystem.current);
        
        // check if mouse is over the gameobject, and if so, disable all watch edge buttons
        
        if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition))
        {
            _isMouseOver = true;
            foreach (var button in watchEdgeButtons)
            {
                button.GetComponent<Button>().interactable = false;
            }
            
            ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.pointerClickHandler);
            }
            
        }
        else if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition) && _isMouseOver)
        {
            _isMouseOver = false;
            ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.pointerExitHandler);
            foreach (var button in watchEdgeButtons)
            {
                button.GetComponent<Button>().interactable = true;
            }
        }
        
        
        
    }
}
