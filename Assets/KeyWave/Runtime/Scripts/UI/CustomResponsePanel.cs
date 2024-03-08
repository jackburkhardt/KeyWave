using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;
using StandardUIResponseButton = PixelCrushers.DialogueSystem.Wrappers.StandardUIResponseButton;

public class CustomResponsePanel : MonoBehaviour
{
    [SerializeField] private UITextField timeEstimate;
    [SerializeField] Animator responseMenuAnimator;
    [SerializeField] private PointerArrow mousePointerHand;
    private DialogueEntry currentlySelectedDialogueEntry;
    private StandardUIResponseButton currentlySelectedButton;
    
    private List<StandardUIResponseButton> nonSelectedButtons {
        get
        {
            return GameObject.FindObjectsOfType<StandardUIResponseButton>().ToList().FindAll(button => button != currentlySelectedButton);
        }
}
    private Color defaultDisabledColor, defaultTextColor;

  

    private void OnEnable()
    {
        Points.OnAnimationStart+= StartPointsAnimation;
        Points.OnAnimationComplete += FinishPointsAnimation;
        
    }

    private void OnDisable()
    {
        Points.OnAnimationStart -= StartPointsAnimation;
        Points.OnAnimationComplete -= FinishPointsAnimation;
    }

    public void PauseDialogueSystem()
    {
        DialogueManager.Pause();
    }
    
    
    public void SetCurrentResponseButton(StandardUIResponseButton responseButton)
    {
        currentlySelectedButton = responseButton;
        currentlySelectedDialogueEntry = responseButton.response.destinationEntry;
       
        if (Field.FieldExists(currentlySelectedDialogueEntry.fields, "Time Estimate")) ShowTimeEstimate();
        else HideTimeEstimate();
    }

    public void SetCurrentResponseButton(Transform transform)
    {
        var responseButton = transform.GetComponent<StandardUIResponseButton>();
        SetCurrentResponseButton(responseButton);
    }

    private void ShowTimeEstimate()
    {
        timeEstimate.gameObject.SetActive(true);
        timeEstimate.text = CalculateTimeEstimate(currentlySelectedDialogueEntry);
    }

    private void HideTimeEstimate()
    {
        timeEstimate.gameObject.SetActive(false);
    }
    
    

    private void SetButtonDisabledColor(Button button, Color color)
    {
        defaultDisabledColor = button.colors.disabledColor;
        var block = button.colors;
        block.disabledColor = color;
        button.colors = block;
    }

    private void HideNonSelectedButtonsText()
    {
        
    }


    private void StartPointsAnimation(Points.Type pointsType)
    {
        SetButtonDisabledColor(currentlySelectedButton.GetComponent<Button>(), Points.Color(pointsType));
        HideTimeEstimate();
        mousePointerHand.Freeze();
        responseMenuAnimator.SetBool("Points", true);
        responseMenuAnimator.Play("Points");
        
    }
    
    private void FinishPointsAnimation()
    {
        if (currentlySelectedButton != null) SetButtonDisabledColor(currentlySelectedButton.GetComponent<Button>(), defaultDisabledColor);
        responseMenuAnimator.SetBool("Points", false);
        mousePointerHand.Unfreeze();
    }

    public void SendSequencerMessage(string message)
    {
        Sequencer.Message(message);
    }
    
    private Vector2 ResponseButtonPosition(StandardUIResponseButton responseButton)
    {
        return responseButton.label.gameObject.transform.position;
    }
    
    public void OnClick()
    {
        if (!currentlySelectedButton.isButtonActive) return;
        foreach (var field in currentlySelectedDialogueEntry.fields)
        {
            if (field.title == "Points")
            {
                Points.SetSpawnPosition(ResponseButtonPosition(currentlySelectedButton));
                GameEvent.OnPointsIncrease(DialogueUtility.GetPointsField(field).type, DialogueUtility.GetPointsField(field).points);
            }
        }
    }
    
    public void UnsetResponseButton()
    {
        var anyButtonActive = false;

        foreach (var responseButton in GetComponentsInChildren(typeof(StandardUIResponseButton)))
        {
            if (responseButton.GetComponent<StandardUIResponseButton>().isButtonActive)
            {
                anyButtonActive = true;
            }
        }
        
        if (!anyButtonActive) HideTimeEstimate();
    }

    public string CalculateTimeEstimate(DialogueEntry dialogueEntry)
    {
        var minTimeEstimate = int.MaxValue;
        var maxTimeEstimate = 0;

        var output = string.Empty;
        
        foreach (var field in dialogueEntry.fields)
        {
            if (field.title == "Time Estimate" && field.type == FieldType.Node)
            {
                var entry = DialogueUtility.GetDialogueEntryFromNodeField(field);
                var timeEstimate = DialogueUtility.DurationRangeBetweenNodes(dialogueEntry, entry);
                
                if (timeEstimate.Item1 < minTimeEstimate) minTimeEstimate = timeEstimate.Item1;
                if (timeEstimate.Item2 > maxTimeEstimate) maxTimeEstimate = timeEstimate.Item2;
            }
        }
        
        if (minTimeEstimate == maxTimeEstimate)
        {
            output = (minTimeEstimate/60).ToString();
        }
        else
        {
            output = minTimeEstimate/60 + " - " + maxTimeEstimate/60;
        }
        
        output += " minutes";
        return output;
    }
}
