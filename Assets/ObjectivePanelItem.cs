using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using UnityEngine.UI;

public class ObjectivePanelItem : MonoBehaviour
{
   
    public QuestState questState;
    public UITextField questTitle;
    public Image tickImage, boxImage;

    public enum Day
    {
        Day1,
        Day2,
        Day3
    }

    public Day day;


    [Foldout("Active")] [Label("Text Color")] public Color textColorActive;
    [Foldout("Active")] [Label("Tick Sprite")] public Sprite tickSpriteActive;
    
    [Foldout("Failure")] [Label("Text Color")] public Color textColorFailure;
    [Foldout("Failure")] [Label("Tick Sprite")] public Sprite tickSpriteFailure;
    
    [Foldout("Success")] [Label("Text Color")] public Color textColorSuccess;
    [Foldout("Success")] [Label("Tick Sprite")] public Sprite tickSpriteSuccess;
    
    
 
    
    // Start is called before the first frame update


    private void OnEnable()
    {
        var allQuests = QuestLog.GetAllQuests();
        
        foreach (var quest in allQuests)
        {
            if (QuestLog.GetQuestState(quest) != QuestState.Success) continue;
            
        }
        
        
    }
}
