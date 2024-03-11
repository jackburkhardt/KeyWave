using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivePanel : MonoBehaviour
{
    
    [SerializeReference] private Image panel;
    [SerializeReference] private ObjectivePanelItem objectivePanelItem;
    public enum Day
    {
        Day1 = 1,
        Day2 = 2,
        Day3 = 3
    }
    
    public Day day;

    public Color activeColor, inactiveColor;
    
    // Start is called before the first frame update
    private void OnEnable()
    {
        objectivePanelItem.gameObject.SetActive(false);
        var currentDay = DialogueLua.GetVariable("day").asInt;
        if ((int) this.day <= currentDay) ShowActivePanel();
        else ShowInactivePanel(); 
    }

    private void ShowActivePanel()
    {
        panel.color = activeColor;
        var allQuests = QuestLog.GetAllQuests();

        var successfulQuests = new List<string>();
        var failedQuests = new List<string>();
        var activeQuests = new List<string>();
        
        foreach (var quest in allQuests)
        {
            
            var questDay = DialogueLua.GetQuestField(quest, "Day").asInt;
            if (questDay != (int) day) continue;
            if (QuestLog.GetQuestState(quest) == QuestState.Success) successfulQuests.Add(quest);
            else if (QuestLog.GetQuestState(quest) == QuestState.Failure) failedQuests.Add(quest);
            else activeQuests.Add(quest);
        }

        foreach (var successfulQuest in successfulQuests)
        {
            var questItem = Instantiate(objectivePanelItem, this.transform);
            questItem.gameObject.SetActive(true);
            questItem.SuccessState(successfulQuest);
        }
        
        foreach (var failedQuest in failedQuests)
        {
            var questItem = Instantiate(objectivePanelItem, this.transform);
            questItem.gameObject.SetActive(true);
            questItem.FailureState(failedQuest);
        }
        
        foreach (var activeQuest in activeQuests)
        {
            var questItem = Instantiate(objectivePanelItem, this.transform);
            questItem.gameObject.SetActive(true);
            questItem.ActiveState(activeQuest);
        }
        
        RefreshLayoutGroups.Refresh(this.gameObject);

    }

    private void ShowInactivePanel()
    {
        panel.color = inactiveColor;
        foreach (var child in this.transform)
        {
            var childTransform = (Transform) child;
            Destroy(childTransform.gameObject);
        }
    }
}
