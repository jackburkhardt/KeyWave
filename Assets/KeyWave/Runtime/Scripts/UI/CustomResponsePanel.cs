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
    [SerializeField] private UITextField timeEstimate;
    [SerializeField] Animator responseMenuAnimator;
    [SerializeField] private PointerArrow mousePointerHand;

    private List<StandardUIResponseButton> ResponseButtons => FindObjectsOfType<StandardUIResponseButton>().ToList();
    private List<StandardUIResponseButton> NonSelectedButtons  {
        get
        {
            return ResponseButtons.FindAll(button => button != currentlySelectedResponseButton.Button);
        }
    }

    private Color defaultDisabledColor, defaultTextColor, defaultHoverColor;
    

    private ResponseButton currentlySelectedResponseButton = new ResponseButton(null);

    private struct ResponseButton
    {
        private StandardUIResponseButton StandardUIResponseButton;
        public StandardUIResponseButton Button => StandardUIResponseButton;
        public Button UnityButton => StandardUIResponseButton?.GetComponent<Button>();
        public DialogueEntry DialogueEntry => StandardUIResponseButton.response.destinationEntry;
        
        public CircularUIButton CircularUIButton => StandardUIResponseButton.GetComponent<CircularUIButton>();

        public Vector2 Position => StandardUIResponseButton.label.gameObject.transform.position;
        public string TimeEstimate => WasVisited ? string.Empty : TimeEstimateText(DialogueEntry);
        public Points.Type PointsType => DialogueUtility.GetPointsField(DialogueEntry).type;
        
        public Color DefaultDisabledColor;
        public Color DefaultHighlightColor;
        
       // public bool MarkAsVisited => Field.LookupBool(DialogueEntry.fields, "Mark Visited");
       
        public string VisitedCheck => Field.Lookup(DialogueEntry.fields, "Visit Var")?.value;
        
        public bool WasVisited => DialogueLua.GetVariable(VisitedCheck, false);
        
        public ResponseButton(StandardUIResponseButton standardUiResponseButton) : this()
        {
            if (standardUiResponseButton == null) return;
            StandardUIResponseButton = standardUiResponseButton;
            DefaultDisabledColor = UnityButton.colors.disabledColor;
            DefaultHighlightColor = UnityButton.colors.highlightedColor;
        }
    }
  

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
        OnButtonDeselection();
            
        currentlySelectedResponseButton = new ResponseButton(responseButton);

        OnButtonSelection();
    }

    public void OnResponsePanelChange()
    {
        foreach (var responseButton in ResponseButtons)
        {
            var button = new ResponseButton(responseButton);
            button.CircularUIButton.ButtonColor = DialogueUtility.NodeColor(button.DialogueEntry);
        }
    }
    

    private void OnButtonSelection()
    {
        if (currentlySelectedResponseButton.Button == null) return;
        timeEstimate.text = currentlySelectedResponseButton.TimeEstimate;
        
        if (currentlySelectedResponseButton.PointsType != Points.Type.Null && !currentlySelectedResponseButton.WasVisited) SetButtonColors(Points.Color(currentlySelectedResponseButton.PointsType), Points.Color(currentlySelectedResponseButton.PointsType));
    }

    private void OnButtonDeselection()
    {
        if (currentlySelectedResponseButton.Button == null) return;
        ResetButtonColors();
        timeEstimate.text = "";
    }
    
    private void StartPointsAnimation(Points.Type pointsType)
    {
        
        mousePointerHand.Freeze();
        responseMenuAnimator.SetBool("Points", true);
        responseMenuAnimator.Play("Points");
        
    }

    private void SetButtonColors(Color highlightColor, Color disabledColor)
    {
        var block = currentlySelectedResponseButton.UnityButton.colors;
        block.highlightedColor = Color.Lerp(currentlySelectedResponseButton.DefaultHighlightColor, highlightColor, 0.3f);
        block.disabledColor =    block.disabledColor = Color.Lerp(currentlySelectedResponseButton.DefaultDisabledColor, disabledColor, 0.9f);
        currentlySelectedResponseButton.UnityButton.colors = block;
    }
    
    private void ResetButtonColors() =>  SetButtonColors(currentlySelectedResponseButton.DefaultHighlightColor, currentlySelectedResponseButton.DefaultDisabledColor);
    
    
    private void FinishPointsAnimation()
    {
        OnButtonDeselection();
        ResetButtonColors();
        responseMenuAnimator.SetBool("Points", false);
        mousePointerHand.Unfreeze();
        
            // if (currentlySelectedResponseButton.MarkAsVisited) currentlySelectedResponseButton.DialogueEntry.fields.Remove(
          //  Field.Lookup(currentlySelectedResponseButton.DialogueEntry.fields, "Points"));
       
    }

    public void SendSequencerMessage(string message)
    {
        Sequencer.Message(message);
    }
    
    public void OnClick()
    {
        if (!currentlySelectedResponseButton.Button.isButtonActive) return;
        
        /*
        
        if (currentlySelectedResponseButton.MarkAsVisited)
        {
          Field.SetValue(currentlySelectedResponseButton.DialogueEntry.fields, "Visited", true);
        }
        
        */
        
       if (currentlySelectedResponseButton.PointsType != Points.Type.Null && !currentlySelectedResponseButton.WasVisited)
        {
            Points.SetSpawnPosition(currentlySelectedResponseButton.Position);
            GameEvent.OnPointsIncrease(currentlySelectedResponseButton.PointsType, DialogueUtility.GetPointsField(currentlySelectedResponseButton.DialogueEntry).points);
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
        if (!anyButtonActive) OnButtonDeselection();
    }

    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        var minTime = estimate.Item1 / 60;
        var maxTime = estimate.Item2 / 60;
        
        if (minTime > maxTime) return "";
        if (minTime == maxTime) return $"{minTime} minutes";
        return $"{minTime}-{maxTime} minutes";
    }
}
