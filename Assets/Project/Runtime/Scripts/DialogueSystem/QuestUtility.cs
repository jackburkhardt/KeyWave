using System.Collections.Generic;
using JetBrains.Annotations;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public class QuestUtility
    {
        public static Points.PointsField[] GetPoints(string questTitle, DialogueDatabase database = null)
        {
            database ??= DialogueManager.MasterDatabase;
            return DialogueUtility.GetPointsFromField(database.GetQuest(questTitle)?.fields);
        }
            

        public static Points.PointsField[] GetPoints(DialogueEntry dialogueEntry, [CanBeNull] DialogueDatabase database = null)
        {
            var conversation = dialogueEntry.GetConversation(database);
            if (conversation.IsUnityNull())
            {
                return null;
            }
            return GetPoints(dialogueEntry.GetConversation(database).Title, database);
        }


        public static Lua.Result GetField(string questTitle, string questField)
        {
            return DialogueLua.GetQuestField(questTitle, questField);
        }

        public static List<Item> GetAllQuessts()
        {
            var quests = DialogueManager.masterDatabase.items.FindAll(i => i.IsItem == false);
            return quests;
        }

        public static List<Item> GetQuestsByGroup(string groupName)
        {
            var quests = GetAllQuessts().FindAll(i => i.Group == groupName);
            return quests;
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
}