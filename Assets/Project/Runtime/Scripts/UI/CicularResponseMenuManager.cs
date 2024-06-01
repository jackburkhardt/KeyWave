using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CicularResponseMenuManager : MonoBehaviour
{
    private Button[] menuItems;
    [SerializeField] private RectTransform responseMenuPanel;
    [SerializeField] private Transform textContainer;
    [SerializeField] private float innerRadiusOffset;
    [SerializeField] private RectTransform pointerArrow;
    public float textPadding;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

   

    private List<float> _thetaRange = new List<float>();

    // Update is called once per frame
    void Update()
    {
        menuItems = transform.GetComponentsInChildren<Button>();
        _thetaRange.Clear();
        var rectTransform = GetComponent<RectTransform>();


        float totalTheta = 0;
        float radius = rectTransform.rect.width / 2f;
       
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            
            var buttonRectTransform = menuItems[i].GetComponent<RectTransform>();
            
            //stretch  button rect transform  to fit parent
            
            
            var m_textContainer = menuItems[i].transform.Find(textContainer.transform.name);

            var textRectTransform = m_textContainer.GetChild(0).GetComponent<RectTransform>();

            var buttonImage = buttonRectTransform.GetComponent<Image>();

            var cordLength = textRectTransform.rect.width + textPadding;
            
            var arcLength = 2f * radius * Mathf.Sin(cordLength / (2f * radius));


            buttonImage.fillAmount = arcLength / (2f * Mathf.PI * radius);
            
            
            buttonRectTransform.pivot = new Vector2(0.5f, 0.5f);
            //set anchor preset to bottom center
            
            textRectTransform.anchorMin = new Vector2(0.5f, 0);
            textRectTransform.anchorMax = new Vector2(0.5f, 0);
            
            var theta = buttonImage.fillAmount * 2 * MathF.PI;
            var degreesToRotate = (totalTheta) * 180 / Mathf.PI;
            
            var xPos = Mathf.Cos(totalTheta) * radius;
            var yPos = Mathf.Sin(totalTheta) * radius;
            
           // rectTransform.localPosition = new Vector3(xPos, yPos,0);
           buttonRectTransform.localRotation = Quaternion.Euler(0, 0, degreesToRotate);
           m_textContainer.localRotation = Quaternion.Euler(0, 0, theta * 180 / Mathf.PI / 2f);
           m_textContainer.localPosition = new Vector3(0, 0, 0);

           
           totalTheta += theta;
           
           _thetaRange.Add(totalTheta);
           menuItems[i].GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }
        
        responseMenuPanel.transform.localRotation = Quaternion.Euler(0, 0,  -(totalTheta * 180 / Mathf.PI / 2f) );

        
        var translatedCursorTheta = pointerArrow.localEulerAngles.z * Mathf.Deg2Rad + totalTheta / 2f;
        if (pointerArrow.localEulerAngles.z > 180) { translatedCursorTheta -= 2 * Mathf.PI;} 
        var pointer = new PointerEventData(EventSystem.current);
        
        var lowerBound = 0f;
        Button ActiveButton = null;
        var isMouseOver = pointerArrow.GetComponent<WatchHandCursor>().isMouseOver;
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            
            if (translatedCursorTheta < _thetaRange[i] && translatedCursorTheta > lowerBound && translatedCursorTheta < totalTheta && isMouseOver)
            {
                ExecuteEvents.Execute(menuItems[i].gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                ActiveButton = menuItems[i];
            }

            else 
            {
                ExecuteEvents.Execute(menuItems[i].gameObject, pointer, ExecuteEvents.pointerExitHandler);
            }
            
            lowerBound = _thetaRange[i];
        }
        
        
        
        if (ActiveButton != null && isMouseOver && Input.GetMouseButtonDown(0))
        {
            ExecuteEvents.Execute(ActiveButton.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        
        
    }

    public void DestroyResponseButtons()
    {
        foreach (var button in transform.GetComponentsInChildren<Button>())
        {
            if (button.transform.GetSiblingIndex() == 0) continue;
            
            DestroyImmediate(button.gameObject);
            
        }
    }
}
