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
    public StandardUIResponseButton buttonTemplate => GetComponent<StandardUIMenuPanel>().buttonTemplate as StandardUIResponseButton;
    
    public static CustomResponsePanel Instance;

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
        public string TimeEstimate => TimeEstimateText(DialogueEntry);
        public Points.Type PointsType => DialogueUtility.GetPointsField(DialogueEntry).Type;
        
        public Color DefaultDisabledColor;
        public Color DefaultHighlightColor;
        
        public void SetButtonColors(Color highlightColor, Color disabledColor)
        {
            var block = UnityButton.colors;
            block.highlightedColor = Color.Lerp(DefaultHighlightColor, highlightColor, 0.3f);
            block.disabledColor =    block.disabledColor = Color.Lerp(DefaultDisabledColor, disabledColor, 0.9f);
            UnityButton.colors = block;
        }

        public void SetButtonColors(Color color) => SetButtonColors(color, color);
        public void ResetButtonColors()
        {
            var block = UnityButton.colors;
            block.highlightedColor = DefaultHighlightColor;
            block.disabledColor = DefaultDisabledColor;
            UnityButton.colors = block;
        }
        
        public Color CurrentDisabledColor => UnityButton.colors.disabledColor;
        public Color CurrentHighlightColor => UnityButton.colors.highlightedColor;
        public ResponseButton(StandardUIResponseButton standardUiResponseButton) : this()
        {
            if (standardUiResponseButton == null) return;
            StandardUIResponseButton = standardUiResponseButton;
            DefaultDisabledColor = UnityButton.colors.disabledColor;
            DefaultHighlightColor = UnityButton.colors.highlightedColor;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        else if (Instance != this)
        {
            Destroy(this);
        }
    }
    
    
    public void SetCurrentResponseButton(StandardUIResponseButton responseButton)
    {
        OnButtonExit();
            
        currentlySelectedResponseButton = new ResponseButton(responseButton);

        OnButtonHover();
    }

    public void OnResponsePanelChange()
    {
        mousePointerHand.Unfreeze();
        
        foreach (var responseButton in ResponseButtons)
        {
            var button = new ResponseButton(responseButton);
            button.CircularUIButton.ButtonColor = DialogueUtility.NodeColor(button.DialogueEntry);
        }
    }
    

    private void OnButtonHover()
    {
        if (currentlySelectedResponseButton.Button == null) return;
        timeEstimate.text = currentlySelectedResponseButton.TimeEstimate;
        
        if (currentlySelectedResponseButton.PointsType != Points.Type.Null) currentlySelectedResponseButton.SetButtonColors(Points.Color(currentlySelectedResponseButton.PointsType), Points.Color(currentlySelectedResponseButton.PointsType));
    }

    private void OnButtonExit()
    {
        if (currentlySelectedResponseButton.Button == null) return;
        currentlySelectedResponseButton.ResetButtonColors();
        timeEstimate.text = "";
    }
    
    private void StartPointsAnimation(Points.Type pointsType)
    {
        
      //  mousePointerHand.Freeze();
       // responseMenuAnimator.SetBool("Points", true);
      //  responseMenuAnimator.Play("Points");
        
    }


    
    private void FinishPointsAnimation()
    {
        OnButtonExit();
        currentlySelectedResponseButton.ResetButtonColors();
        responseMenuAnimator.SetBool("Points", false);
        mousePointerHand.Unfreeze();
       
    }

    public void SendSequencerMessage(string message)
    {
        Sequencer.Message(message);
    }
    
    public void OnClick()
    {
        
       if (!currentlySelectedResponseButton.Button.isButtonActive) return;
       currentlySelectedResponseButton.SetButtonColors(currentlySelectedResponseButton.CurrentHighlightColor);
       
       foreach (var button in NonSelectedButtons)
       {
           button.label.color = Color.clear;
       }
       
       mousePointerHand.Freeze();
        
       if (currentlySelectedResponseButton.PointsType != Points.Type.Null)
        {
            Points.SetSpawnPosition(currentlySelectedResponseButton.Position);
            GameEvent.OnPointsIncrease(DialogueUtility.GetPointsField(currentlySelectedResponseButton.DialogueEntry));
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
        if (!anyButtonActive) OnButtonExit();
    }

    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        if (!Field.FieldExists(dialogueEntry.fields, "Time Estimate")) return "";
        
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        if (estimate.Item1 > estimate.Item2) return "";
        if (estimate.Item1 == estimate.Item2) return $"{estimate.Item1 / 60} minutes";
        return $"{estimate.Item1 / 60}-{estimate.Item2 / 60} minutes";
    }
}
