using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using Field = PixelCrushers.DialogueSystem.Field;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public static class DialogueUtility
    {
        public enum QuestState
        {
            unassigned,
            active,
            failure,
            success,
            done,
            abandoned,
            grantable,
            returnToNPC,
        }

        public static DialogueEntry CurrentDialogueEntry =>
            DialogueManager.instance.currentConversationState.subtitle.dialogueEntry;

        public static int CurrentNodeDuration => GetNodeDuration(CurrentDialogueEntry);

        public static bool Empty(DialogueEntry node)
        {
            return node.currentDialogueText == string.Empty && node.currentMenuText == string.Empty;
        }


        public static Color NodeColor(DialogueEntry node)
        {
            var visitedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            var leaveColor = new Color(0.2f, 0.05f, 0.05f, 1f);
            var backColor = new Color(0.5f, 0.5f, 0.5f, 1);
            var proceduralColor = new Color(1, 1, 1, 1);
            var defaultColor = Location.PlayerLocation.responseMenuButtonColor;
        
            if (node == null) return Color.white;

            // if (Field.FieldExists(node.fields, "Visit Var") && DialogueLua.GetVariable(Field.Lookup(node.fields, "Visit Var").value, false)) 
            //    return Color.Lerp(visitedColor, defaultColor, 0.4f);
            if (node.Title == "Leave") return Color.Lerp(leaveColor, defaultColor, 0.2f);
            if (node.Title == "Back") return Color.Lerp(backColor, defaultColor, 0.2f);
      
            if (node.GetSubconversationQuest()!= null && node.GetSubconversationQuest()!.FieldExists("Is Procedural") && node.GetSubconversationQuest()!.GetLuaField("Is Procedural")!.Value.asBool)  return Color.Lerp(proceduralColor, defaultColor, 0.2f);
            return defaultColor;
        }

        public static Points.PointsField[] GetPointsFromField(List<Field> fields)
        {
            if (fields == null || !Field.FieldExists(fields, "Points")) return Array.Empty<Points.PointsField>();
            var pointsField = fields.Where(f => f.title == "Points");
            return pointsField.Select(field => Points.PointsField.FromString(field.value)).ToArray();
        }

        public static Item GetQuestByName(string questName)
        {
            return DialogueManager.Instance.masterDatabase.items.Find(item => item.Name == questName);
        }

        public static int GetTimespan(DialogueEntry dialogueEntry)
        {
            if (!Field.FieldExists(dialogueEntry.fields, "Timespan")) return -1;
        
            var timespanField = Field.Lookup(dialogueEntry.fields, "Timespan");
        
            var value = timespanField.value.Split(':')[0] == null ? 0 : int.Parse(timespanField.value.Split(':')[0]);

            var unit = timespanField.value.Split(':')[1];

            switch (unit)
            {
                case "seconds":
                    break;
                case "minutes":
                    value *= 60;
                    break;
                case "hours":
                    value *= 3600;
                    break;
            }

            return value * Clock.TimeScales.GlobalTimeScale;
        }

        public static int GetQuestDuration(Item quest)
        {
            var durationField = quest.AssignedField("Duration");
            // if (quest.GetQuestState() == PixelCrushers.DialogueSystem.QuestState.Success) return 0;
            if (durationField == null) return 0;
        
            var unit = durationField.value.Split(':')[1];
            var questTime = int.Parse(durationField.value.Split(':')[0]);
            var duration = 0;

            if (unit == "seconds") duration = questTime;
            else if (unit == "minutes") duration = questTime * 60;
            else if (unit == "hours") duration = questTime * 3600;
        
            return duration;
        }

        private static List<List<DialogueEntry>> FindAllPathsBetweenNodes(DialogueEntry node1, DialogueEntry node2)
        {

            var stack = new List<DialogueEntry>();
            var visited = new List<DialogueEntry>();
            var paths = new List<List<DialogueEntry>>();

            var currentNode = node1;

            stack.Add(currentNode);

            // get all paths from node1 to node2 using DFS algorithm


            void DFS(DialogueEntry node)
            {
                if (node == node2)
                {
                    paths.Add(new List<DialogueEntry>(stack));
                    return;
                }

                visited.Add(node);

                foreach (var link in node.outgoingLinks)
                {
                    var nextNode = GetDialogueEntryByID(link.destinationConversationID, link.destinationDialogueID);
                    if (visited.Contains(nextNode)) continue;

                    stack.Add(nextNode);
                    DFS(nextNode);
                    stack.Remove(nextNode);
                }
            }

            DFS(currentNode);
        
            return paths;
        
        }

        private static int FindShortestDurationBetweenPaths(List<List<DialogueEntry>> paths)
        {
            var shortestDistance = int.MaxValue;
        
            foreach (var path in paths)
            {
                var distance = 0;
            
                for (int i = 0; i < path.Count; i++)
                {
                    distance += GetNodeDuration(path[i].conversationID, path[i].id);
                }
            
                if (distance < shortestDistance) shortestDistance = distance;
            }
            return shortestDistance;
        }

        private static int FindLargestDurationBetweenPaths(List<List<DialogueEntry>> paths)
        {
            var largestDistance = 0;
        
            foreach (var path in paths)
            {
                var distance = 0;
            
                for (int i = 0; i < path.Count; i++)
                {
                    distance += GetNodeDuration(path[i].conversationID, path[i].id);
                }
            
                if (distance > largestDistance) largestDistance = distance;
            }
        
            return largestDistance;
        }

        public static (int, int) DurationRangeBetweenNodes(DialogueEntry node1, DialogueEntry node2)
        {
            var paths = FindAllPathsBetweenNodes(node1, node2);
            var shortest = FindShortestDurationBetweenPaths(paths);
            var largest = FindLargestDurationBetweenPaths(paths);
            return (shortest, largest);
        }

        public static (int, int) TimeEstimate(DialogueEntry node)
        {
            var minTimeEstimate = int.MaxValue;
            var maxTimeEstimate = 0;
        
        
            foreach (var field in node.fields)
            {
                if (field.title == "Time Estimate" && field.type == FieldType.Node)
                {
                    if (field.value == "0,0") continue;
                    var entry = GetDialogueEntryFromNodeField(field);
                    var timeEstimate = DurationRangeBetweenNodes(node, entry);
                
                    if (timeEstimate.Item1 < minTimeEstimate) minTimeEstimate = timeEstimate.Item1;
                    if (timeEstimate.Item2 > maxTimeEstimate) maxTimeEstimate = timeEstimate.Item2;
                }
            }

            return (minTimeEstimate, maxTimeEstimate);
        }


        public static DialogueEntry GetDialogueEntryByID(int conversationID, int entryID) => GetDialogueEntryByID(GetConversationByID(conversationID), entryID);

        public static DialogueEntry GetDialogueEntryByID(Conversation conversation, int id)
        {
            return conversation.dialogueEntries.Find(
                entry => entry.id == id);
        }

        public static DialogueEntry GetDialogueEntryByLink(Link outgoingLink)
        {
            return GetDialogueEntryByID( outgoingLink.destinationConversationID, outgoingLink.destinationDialogueID);
        }

        public static DialogueEntry GetDialogueEntryFromNodeField(Field field)
        {
            if (field.type != FieldType.Node) return null;
        
            var conversationID = field.value.Split(',')[0] == null ? 0 : int.Parse(field.value.Split(',')[0]);
        
            var entryID = field.value.Split(',')[1] == null ? 0 : int.Parse(field.value.Split(',')[1]);
        
            return GetDialogueEntryByID(conversationID, entryID);
        }


        public static Conversation GetConversationByID(int id)
        {
            return DialogueManager.masterDatabase.conversations.Find(
                conversation => conversation.id == id);
        }

        public static Conversation GetConversationByDialogueEntry(DialogueEntry dialogueEntry)
        {
            return GetConversationByID(dialogueEntry.conversationID);
        }

        private static int GetLineAutoDuration(string line)
        {

            if (line == string.Empty)
            {
            
                return 0;
            }
            //Debug.Log($"Auto Node Duration from Line: {line.Length / Clock.TimeScales.SpokenCharactersPerSecond + Clock.TimeScales.SecondsBetweenLines}");
            return ((int)(line.Length * Clock.TimeScales.SecondsPercharacter) +
                    Clock.TimeScales.SecondsBetweenLines) * Clock.TimeScales.GlobalTimeScale;
        }

        public static int GetNodeDuration(DialogueEntry dialogueEntry)
        {
            var timespan = GetTimespan(dialogueEntry);
            var t = dialogueEntry.Timespan();
            if (timespan != -1)
            {
                /*Debug.Log($"Node Duration from Timespan: {timespan}"); */
                return timespan;
            }

            if (Field.FieldExists(dialogueEntry.fields, "Duration"))
            {
                //Debug.Log($"Node Duration from Duration Field: {Field.LookupInt(dialogueEntry.fields, "Duration")}");
                return Field.LookupInt(dialogueEntry.fields, "Duration");
            }
        
            if (dialogueEntry.Visited()) return 0;
        
            return GetLineAutoDuration(dialogueEntry.currentDialogueText);
        }

        public static int GetNodeDuration(int conversationID, int nodeID)
        {
            var node = GetDialogueEntryByID(conversationID, nodeID);
            return GetNodeDuration(node);
        }
    }
}