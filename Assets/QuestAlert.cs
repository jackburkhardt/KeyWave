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

    void Alert(string startMessage, string title, string description)
    {
        var alert = $"<size=90%><i>New Task: </i></size>{title}</i><br><size=80%><b>{description}</b></size>";
        DialogueManager.ShowAlert(alert);
    }

    void OnQuestStateChange(string questName)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == questName);
        if (quest.Group != "Main Task") return;
        if (QuestLog.GetQuestState(questName) != QuestState.Active) return;
        
        var title = QuestLog.GetQuestTitle(questName); 
        var questDescription = QuestLog.FindQuestEntryByState(questName, QuestState.Active); 
        Alert("New Task", title, questDescription);
    }

    void OnQuestEntryStateChange(QuestEntryArgs args)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == args.questName);
        if (quest.Group != "Main Task") return;
        var questEntry = QuestLog.GetQuestEntry(args.questName, args.entryNumber);
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
