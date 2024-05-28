using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class QuestUtility
{

    public static Points.PointsField GetPoints(string questTitle)
    {
        var points = GetField(questTitle, "Points").asString;
        if (string.IsNullOrEmpty(points)) return new Points.PointsField {Type = Points.Type.Null, Points = 0};
        var pointsValue = int.Parse(points.Split(':')[1]);
        var pointsType = pointsValue == 0 ? Points.Type.Null : (Points.Type) Enum.Parse(typeof(Points.Type), points.Split(':')[0]);
        return new Points.PointsField {Type = pointsType, Points = pointsValue};
    }
    
    public static Points.PointsField GetPoints(DialogueEntry dialogueEntry)
    {
        return GetPoints(DialogueUtility.GetConversationByDialogueEntry(dialogueEntry).Title);
    }


    public static Lua.Result GetField(string questTitle, string questField)
    {
        return DialogueLua.GetQuestField(questTitle, questField);
    }
 
    
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
