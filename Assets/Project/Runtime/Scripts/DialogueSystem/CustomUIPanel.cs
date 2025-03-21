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
    
    public string forceCloseAnimationTrigger;

    private Animator Animator => animator ? animator : GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

    protected void Awake()
    {
        OnAwake?.Invoke();
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
    
    public void ForceClose()
    {
        Close();
        animator.SetTrigger(forceCloseAnimationTrigger);
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
                   // panel.AddFocus();
                }
                else
                {
                  //  panel.RemoveFocus();
                }
            }
        }
    }
    
}


public class SequencerCommandHideCustomPanel : SequencerCommand
{
    private void Awake()
    {
        var panels = FindObjectsOfType<CustomUIPanel>();
        var panelName = GetParameter(0);
        foreach (var panel in panels)
        {
            if (panel.panelName == panelName)
            {
                panel.ForceClose();
            }
        }
    }
    
}