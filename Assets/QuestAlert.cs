using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestAlert : MonoBehaviour
{
    [SerializeField] private Color onQuestActiveColor, onQuestUpdateColor = Color.white;
    [SerializeField] private Image image;
    [SerializeField] private UITextField alertType, taskName;
    public UnityEvent OnShowEvent;
    public UnityEvent OnHideEvent;
    // Start is called before the first frame update
  
    void Alert(string alert, string title, string description)
    {
        alertType.text = alert;
        taskName.text = title;
        DialogueManager.ShowAlert(description);
    }

    void OnQuestStateChange(string questName)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == questName);
        if (quest.Group != "Main Task") return;
        if (QuestLog.GetQuestState(questName) != QuestState.Active) return;
        image.color = onQuestActiveColor;
        var title = QuestLog.GetQuestTitle(questName); 
        var questDescription = QuestLog.FindQuestEntryByState(questName, QuestState.Active); 
        Alert("New Task", title, questDescription);
    }

    void OnQuestEntryStateChange(QuestEntryArgs args)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == args.questName);
        if (quest.Group != "Main Task") return;
        var questEntry = QuestLog.GetQuestEntry(args.questName, args.entryNumber);
        image.color = onQuestUpdateColor;
        Alert("Task Update", QuestLog.GetQuestTitle(args.questName), questEntry);
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
