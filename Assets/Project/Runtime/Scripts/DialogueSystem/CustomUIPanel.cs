using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

public class CustomUIPanel : UIPanel
{
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;
    
    [GetComponent]
    [SerializeField] private Animator animator = null;
    
    public UnityEvent OnFocus;
    public UnityEvent OnUnfocus;

    private Animator Animator => animator ? animator : GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
    
    
    // Start is called before the first frame update
    
    public override void CheckFocus()
    {
        
       
        
        if (topPanel != this)
        {
         
            if (Animator != null && Animator.isInitialized && !string.IsNullOrEmpty(unfocusAnimationTrigger))
            {
                Animator.ResetTrigger(focusAnimationTrigger);
                Animator.SetTrigger(unfocusAnimationTrigger);
                OnUnfocus?.Invoke();
            }
        }
        else
        {
           
            if (Animator != null && Animator.isInitialized && !string.IsNullOrEmpty(unfocusAnimationTrigger))
            {
                Animator.ResetTrigger(unfocusAnimationTrigger);
                Animator.SetTrigger(focusAnimationTrigger);
                OnFocus?.Invoke();
            }
        }
        
        base.CheckFocus();
    }
    
    public void OnConversationEnd()
    {
       
    }

    public void RemoveFocus()
    {
        PopFromPanelStack();
        CheckFocus();
    }
    
    private string currentConversation;
    
    public void OnConversationLine(Subtitle subtitle)
    {
        currentConversation = subtitle.dialogueEntry.GetConversation().Title;
    }
    
    public void OpenSmartWatch()
    {
        if (DialogueManager.instance.IsConversationActive)
        {
            Debug.Log(currentConversation);
            DialogueManager.instance.StopConversation();
            
            if (currentConversation != "SmartWatch/Home")
            {
                DialogueManager.instance.StartConversation("SmartWatch/Home");
            }
            
            else RemoveFocus();
        }
          
        else if (!DialogueManager.instance.IsConversationActive)
        {
            DialogueManager.instance.StartConversation("SmartWatch/Home");
        }
       
    }
    
    void Start()
    {
        
    }

}
