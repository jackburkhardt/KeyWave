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
using StandardUIResponseButton = PixelCrushers.DialogueSystem.Wrappers.StandardUIResponseButton;

public class CustomResponsePanel : MonoBehaviour
{
    public static Action<CustomResponseButton> OnCustomResponseButtonClick;
    public static void ButtonClick(CustomResponseButton button) => OnCustomResponseButtonClick?.Invoke(button);

    public AnimationCurve offsetCurve;
    
    [SerializeField] private UITextField timeEstimate;
    [SerializeField] Animator responseMenuAnimator;
    [SerializeField] private PointerArrow mousePointerHand;
    
    public float globalOffset;

    public StandardUIResponseButton buttonTemplate
    {
        get
        {
            if (GetComponent<StandardUIMenuPanel>().buttonTemplate != null ) return GetComponent<StandardUIMenuPanel>().buttonTemplate as StandardUIResponseButton;
            return null;
        }
    }


    public static CustomResponsePanel Instance;
    private float maxVisibleDegreeSum = 85;

    public float CircularUIDegreeSum
    {
        get
        {
            var degreeSum = 0f;
            foreach (var circularUIButton in FindObjectsOfType<CircularUIButton>())
            {
               degreeSum += circularUIButton.image.fillAmount * 360f;
            }

            return degreeSum;
        }
    }

    private struct ResponseButton
    {
       
     
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OnResponsePanelChange()
    {
        mousePointerHand.Unfreeze();
        CustomResponseButton.RefreshButtonColors();
    }
    
/*
    
    private void StartPointsAnimation(Points.Type pointsType)
    {
        
      //  mousePointerHand.Freeze();
       // responseMenuAnimator.SetBool("Points", true);
      //  responseMenuAnimator.Play("Points");
        
    }


    
    private void FinishPointsAnimation()
    {
        currentlySelectedResponseButton.ResetButtonColors();
        responseMenuAnimator.SetBool("Points", false);
        mousePointerHand.Unfreeze();
       
    }

    public void SendSequencerMessage(string message)
    {
        Sequencer.Message(message);
    }
    
    */
    
    public void OnPlayerResponse(CustomResponseButton button)
    {
        
       mousePointerHand.Freeze();
    }
    
    

    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        if (!Field.FieldExists(dialogueEntry.fields, "Time Estimate")) return "";
        
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        if (estimate.Item1 > estimate.Item2) return "";
        if (estimate.Item1 == estimate.Item2) return $"{estimate.Item1 / 60} minutes";
        return $"{estimate.Item1 / 60}-{estimate.Item2 / 60} minutes";
    }

    public void OnQuestStateChange(string questTitle)
    {
        Debug.Log("questChange");
    }

    private void Update()
    {

        if (CircularUIDegreeSum < maxVisibleDegreeSum) return;

        else
        {
            var normalizedPointerAngle = mousePointerHand.AngleCenteredSouth / maxVisibleDegreeSum * 2f; 

            var offsetRange = CircularUIDegreeSum - maxVisibleDegreeSum;
            
           
           
            var offset = offsetRange * - (offsetCurve.Evaluate(Mathf.Abs(normalizedPointerAngle)) *  MathF.Sign(normalizedPointerAngle) * 0.5f);

            CircularUIButton.globalOffset = offset;


        }
    }
}
