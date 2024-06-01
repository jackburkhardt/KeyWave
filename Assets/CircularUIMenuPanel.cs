using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PixelCrushers;
using UnityEngine;

public class CircularUIMenuPanel : CustomUIMenuPanel
{
    private const float MaxVisibleDegreeSum = 85;
    [SerializeField] private WatchHandCursor mousePointerHand;
    [SerializeField] public AnimationCurve offsetCurve;
    [SerializeField] private UITextField timeEstimate;
    private Animator? Animator => GetComponent<Animator>();
    
   
    public new static List<string> CustomFields = new List<string>
    {
        "offsetCurve", 
        "timeEstimate",
        "responseMenuAnimator",
        "mousePointerHand"
    };
    
    
    public void OnChoiceSelection()
    {
        if (!Animator!.GetBool("Active")) return;
        WatchHandCursor.Freeze();
        Animator!.SetBool("Frozen", true);
    }
    
 

    
    protected override void OnContentChanged()
    {
        _firstFocus = false;
        base.OnContentChanged();
        WatchHandCursor.Unfreeze();
       
        Animator!.SetBool("Frozen", false);
    }

    public void SetPropertiesFromButton(CircularUIResponseButton button)
    {
        if (button == null)
        {
            timeEstimate.text = "";
        }

        else
        {
            timeEstimate.text = button.TimeEstimateText;
        }
    }

    private bool _firstFocus = false;

   
    protected override void Update()
    {
        base.Update();
        
        var circularButtons = GetComponentsInChildren<CircularUIButton>();

        foreach (var circularButton in circularButtons)
        {
            if (circularButton.CircularUIDegreeSum < MaxVisibleDegreeSum)  circularButton.Offset = 0;
            else
            {
                var normalizedPointerAngle = mousePointerHand.AngleCenteredSouth / MaxVisibleDegreeSum * 2f; 
                var offsetRange = circularButton.CircularUIDegreeSum - MaxVisibleDegreeSum;
                var offset = offsetRange * - (offsetCurve.Evaluate(Mathf.Abs(normalizedPointerAngle)) *  MathF.Sign(normalizedPointerAngle) * 0.5f); 
                circularButton.Offset = offset;
            }
        }
        
        var normalizedMousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        
        var normalizedMenuPosition = new Vector2(transform.position.x / Screen.width, transform.position.y / Screen.height);
        
        var distance = Vector2.Distance(normalizedMousePosition, normalizedMenuPosition);
        
        var active = Animator!.GetBool("Active");

        if (!active) return;
        
        if (distance > 1.65f && _firstFocus)
        {
            Animator!.SetBool("Focus", false);
            
        }
        else if (distance < 1.65f)
        {
            Animator!.SetBool("Focus", true);
            _firstFocus = true;
        }
        
      
    }
}
