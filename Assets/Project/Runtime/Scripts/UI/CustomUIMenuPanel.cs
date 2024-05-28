using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;
using UnityEngine.Events;

public class CustomUIMenuPanel : StandardUIMenuPanel
{
    public static List<string> CustomFields = new List<string>
    {
        "offsetCurve", 
        "timeEstimate",
        "responseMenuAnimator",
        "mousePointerHand"
    };
    
    public static Action<CustomUIResponseButton> OnCustomResponseButtonClick;

    public static void ButtonClick(CustomUIResponseButton button)
    {
        Instance.mousePointerHand.Freeze();
        OnCustomResponseButtonClick?.Invoke(button);
    } 

    [SerializeField] public AnimationCurve offsetCurve;
    
    [SerializeField] private UITextField timeEstimate;
    [SerializeField] Animator responseMenuAnimator;
    [SerializeField] private PointerArrow mousePointerHand;
    
   
    public static CustomUIMenuPanel Instance;
    private float maxVisibleDegreeSum = 85;

    public override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
    }

    protected override void OnEnable()
    { 
        base.OnEnable();
       onContentChanged.AddListener(OnContentChanged);
    }

    protected override void OnDisable()
    { 
        base.OnDisable();
       onContentChanged.RemoveListener(OnContentChanged);
    }
    
    private void OnContentChanged()
    {
        mousePointerHand.Unfreeze();
        CustomUIResponseButton.RefreshButtonColors();
        foreach (var button in FindObjectsOfType<CustomUIResponseButton>())
        {
          button.SetResponseText();
        }
    }
    
  

    public void OnQuestStateChange(string questTitle)
    {
       // Debug.Log("questChange");
    }

    public void OnConversationEnd()
    {
        responseMenuAnimator.SetTrigger("Hide");
    }

    public static void OnButtonHover(CustomUIResponseButton button)
    {
        if (button == null)
        {   Instance.timeEstimate.text = "";
            return;
        } Instance.timeEstimate.text = button.TimeEstimateText;
    }

    protected override void Update()
    { 
        base.Update();
        
        
 var circularButtons = GetComponentsInChildren<CircularUIButton>();

        foreach (var circularButton in circularButtons)
        {
            if (circularButton.CircularUIDegreeSum < maxVisibleDegreeSum)  circularButton.Offset = 0;
            else
            {
                var normalizedPointerAngle = mousePointerHand.AngleCenteredSouth / maxVisibleDegreeSum * 2f; 
                var offsetRange = circularButton.CircularUIDegreeSum - maxVisibleDegreeSum;
                var offset = offsetRange * - (offsetCurve.Evaluate(Mathf.Abs(normalizedPointerAngle)) *  MathF.Sign(normalizedPointerAngle) * 0.5f); 
                circularButton.Offset = offset;
            }
        }
    }
}
    

    

