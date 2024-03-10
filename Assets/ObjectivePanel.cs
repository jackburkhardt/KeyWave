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
        var day = DialogueLua.GetVariable("day").asInt;
        if (day <= (int) this.day) ShowActivePanel();
        else ShowInactivePanel(); 
    }

    private void ShowActivePanel()
    {
        var allQuests = QuestLog.GetAllQuests();
        
        foreach (var quest in allQuests)
        {
            
            var questTitle = QuestLog.GetQuestTitle(quest);
           
            
            
        }
        
    }

    private void ShowInactivePanel()
    {
        
    }
}
