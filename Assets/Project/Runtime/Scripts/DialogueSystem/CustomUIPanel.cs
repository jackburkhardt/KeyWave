using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

public class CustomUIPanel : UIPanel
{
    public string panelName;
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;
    
    [GetComponent]
    [SerializeField] private Animator animator = null;
    
    public UnityEvent OnFocus;
    public UnityEvent OnUnfocus;
    
    public UnityEvent OnAwake;

    private Animator Animator => animator ? animator : GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

    protected void Awake()
    {
        OnAwake?.Invoke();
    }


    protected override void Update()
    {
        base.Update();
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            OpenSmartWatch();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Open();
        }
       
    }
    
    
   
    
    // Start is called before the first frame update
    
    public override void CheckFocus()
    {
        
        if (topPanel != this)
        {
         
            if (Animator != null && Animator.isInitialized && !string.IsNullOrEmpty(unfocusAnimationTrigger))
            {
                Animator.SetTrigger(unfocusAnimationTrigger);
                OnUnfocus?.Invoke();
            }
        }
        else
        {
           
            if (Animator != null && Animator.isInitialized && !string.IsNullOrEmpty(focusAnimationTrigger))
            {
                Animator.SetTrigger(focusAnimationTrigger);
                OnFocus?.Invoke();
            }
        }
        
        base.CheckFocus();
    }
    
    public void OnConversationEnd()
    {
        //CheckFocus();
    }

    public void OnUIPanelClose(UIPanel panel)
    {
    }
    

    public void OnUIPanelOpen(UIPanel panel)
    {
        if (GetComponentsInChildren<UIPanel>().Contains(panel))
        {
            TakeFocus();
        }
        
        foreach (var childPanel in childPanels)
        {
            if (childPanel.panel == panel)
            {
                //childPanel.onOpen?.Invoke();
                if (!string.IsNullOrEmpty(childPanel.onOpenAnimationTrigger))
                {
                    Animator.SetTrigger(childPanel.onOpenAnimationTrigger);
                }
                return;
            }
        }
        
        if (!string.IsNullOrEmpty(defaultChildPanelOpenAnimationTrigger) && GetComponentsInChildren<UIPanel>().Contains(panel))
        {
            Animator.SetTrigger(defaultChildPanelOpenAnimationTrigger);
        }
        
    }
    
    public string defaultChildPanelOpenAnimationTrigger;

    [Serializable]
    public struct ChildPanel
    {
        public UIPanel panel;
        public string onOpenAnimationTrigger;
        public string onCloseAnimationTrigger;
        //public UnityEvent onOpen;
    }

    public List<ChildPanel> childPanels;
    
    public void AddFocus()
    {
        PushToPanelStack();
        CheckFocus();
    }

    public void RemoveFocus()
    {
        PopFromPanelStack();
        CheckFocus();
        DialogueManager.instance.StopConversation();
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

}

public class SequencerCommandSetCustomPanel : SequencerCommand
{
    private void Awake()
    {
        var panelName = GetParameter(0);
        var show = GetParameterAsBool(1, true);
        var panels = FindObjectsOfType<CustomUIPanel>();
        foreach (var panel in panels)
        {
            if (panel.panelName == panelName)
            {
                StartCoroutine(Delay(show, panel));
            }
        }
    }
    
    private IEnumerator Delay(bool show, CustomUIPanel panel) 
    {
        yield return new WaitForSeconds(0.5f);
        if (show)
        {
            panel.Open();
        }
        else
        {
            panel.Close();
        }
        
    }
}

public class SequencerCommandFocusCustomPanel : SequencerCommand
{
    private void Awake()
    {
        var panelName = GetParameter(0);
        var show = GetParameterAsBool(1, true);
        var panels = FindObjectsOfType<CustomUIPanel>();
        foreach (var panel in panels)
        {
            if (panel.panelName == panelName)
            {
                if (show)
                {
                    panel.AddFocus();
                }
                else
                {
                    panel.RemoveFocus();
                }
            }
        }
    }
    
}