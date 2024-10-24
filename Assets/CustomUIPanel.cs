using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using Project;
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
                Animator.SetTrigger(unfocusAnimationTrigger);
                OnUnfocus?.Invoke();
            }
        }
        else
        {
           
            if (Animator != null && Animator.isInitialized && !string.IsNullOrEmpty(unfocusAnimationTrigger))
            {
                Animator.SetTrigger(focusAnimationTrigger);
                OnFocus?.Invoke();
            }
        }
        
        base.CheckFocus();
    }

    public void RemoveFocus()
    {
        PopFromPanelStack();
        CheckFocus();
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
