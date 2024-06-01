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
       
    };

    
    [SerializeField] Animator responseMenuAnimator;
   
   
    
    public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();
    

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
    
    protected virtual void OnContentChanged()
    {
        foreach (var button in ResponseButtons)
        {
            if (!button.gameObject.activeSelf && button != buttonTemplate) Destroy(button);
            else button.Refresh();
        }
    }

    public void OnQuestStateChange(string questTitle)
    {
       
    }

    public void OnConversationEnd()
    {
        responseMenuAnimator.SetTrigger("Hide");
    }
    
    
}
    

    

