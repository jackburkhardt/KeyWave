using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class QuestUtility
{

 
    
    /// <summary>
    /// Quest has started but no entries have been completed.
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public static bool QuestInProgressButNascent(string quest)
    {
        return QuestInProgress(quest) && !QuestPartiallyComplete(quest);
    }
    
    /// <summary>
    /// Quest has started and at least one entry has been assigned or completed..
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public static bool QuestInProgress(string quest)
    {
        if (QuestLog.GetQuestState(quest) == QuestState.Success) return false;
        var anyEntryActive = false;
        
        for (int i = 0; i < QuestLog.GetQuestEntryCount(quest); i++)
        {
            if (!anyEntryActive) anyEntryActive = QuestLog.GetQuestEntryState(quest, i) != QuestState.Unassigned;
        }

        return anyEntryActive;
    }
    
    /// <summary>
    /// At least one quest entry has been completed.
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    
    public static bool QuestPartiallyComplete(string quest)
    {
        var anyEntryComplete = false;
        
        for (int i = 0; i < QuestLog.GetQuestEntryCount(quest); i++)
        {
            if (!anyEntryComplete) anyEntryComplete = QuestLog.GetQuestEntryState(quest, i + 1) == QuestState.Success;
        }
        
        return anyEntryComplete;
    }
}
