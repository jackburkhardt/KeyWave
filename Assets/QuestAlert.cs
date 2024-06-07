using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

public class QuestAlert : MonoBehaviour
{
    public UnityEvent OnShowEvent;
    public UnityEvent OnHideEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnQuestStateChange(string questName)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == questName);
        if (quest.Group == "Main Task")
        {
            string alert = String.Empty;
            if (QuestLog.GetQuestState(questName) == QuestState.Active)
            {
                alert = $"<size=80%><i>New Task:</i></size><br>{QuestLog.GetQuestDescription(questName)}";
                DialogueManager.ShowAlert(alert);
            }
            
            
        }
        
    }

    public void OnShow()
    {
        OnShowEvent?.Invoke();
    }
    
    public void OnHide()
    {
        OnHideEvent?.Invoke();
    }
}
