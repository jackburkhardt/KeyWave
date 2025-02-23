using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using Field = PixelCrushers.DialogueSystem.Field;

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
            var defaultColor = Color.white;
        
            if (node == null) return Color.white;

            // if (Field.FieldExists(node.fields, "Visit Var") && DialogueLua.GetVariable(Field.Lookup(node.fields, "Visit Var").value, false)) 
            //    return Color.Lerp(visitedColor, defaultColor, 0.4f);
            if (node.Title == "Leave") return Color.Lerp(leaveColor, defaultColor, 0.2f);
            if (node.Title == "Back") return Color.Lerp(backColor, defaultColor, 0.2f);
      
            if (node.GetSubconversationQuest()!= null && node.GetSubconversationQuest()!.FieldExists("Is Procedural") && node.GetSubconversationQuest()!.GetLuaField("Is Procedural")!.Value.asBool)  return Color.Lerp(proceduralColor, defaultColor, 0.2f);
            return defaultColor;
        }

        public static Points.PointsField[] GetPointsFromField(List<Field> fields, string prefix = "")
        {
            if (fields == null || !fields.Any(p => p.title.EndsWith(" Points"))) return Array.Empty<Points.PointsField>();
            var pointsField = fields.Where(p => p.title.EndsWith(" Points") && p.value.StartsWith(prefix));
            return pointsField.Select(Points.PointsField.FromLuaField).ToArray();
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

            return value;
        }

        public static int GetQuestDuration(Item quest)
        {
            var durationField = quest.AssignedField("Explicit Duration");
            
            if (durationField == null) return 0;
            
            if (quest.AssignedField("Time Flow").value != "Explicit") return 0;
            
            return int.Parse(durationField.value);
        }

        private static List<List<DialogueEntry>> FindAllPathsBetweenNodes(DialogueEntry node1, DialogueEntry node2)
        {

            var stack = new List<DialogueEntry>();
            var visited = new List<DialogueEntry>();
            var paths = new List<List<DialogueEntry>>();

            var currentNode = node1;

            stack.Add(currentNode);


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
            
            var timespan = GetNodeDuration(node);
            
            if (timespan != 0) minTimeEstimate = maxTimeEstimate = timespan;
            
            if (node.Title == "ACTION") return (minTimeEstimate, maxTimeEstimate);
        
        
            //must be a single outgoing link to a different conversation
            if (node.outgoingLinks.Count != 1 || node.outgoingLinks[0].destinationConversationID == node.conversationID) return (minTimeEstimate, maxTimeEstimate);

            var firstNode = node.outgoingLinks[0].GetDestinationEntry();
            
            var finalNodes = new List<DialogueEntry>();
            
            foreach (var nodes in firstNode.GetConversation().dialogueEntries)
            {
                if (nodes.outgoingLinks.Count == 0) finalNodes.Add(nodes);
            }
            
            
            foreach (var finalNode in finalNodes)
            {
                var timeEstimate = DurationRangeBetweenNodes(firstNode, finalNode);
                
                if (timeEstimate.Item1 < minTimeEstimate) minTimeEstimate = timeEstimate.Item1;
                if (timeEstimate.Item2 > maxTimeEstimate) maxTimeEstimate = timeEstimate.Item2;
            }

            if (minTimeEstimate == int.MaxValue) minTimeEstimate = maxTimeEstimate;
            
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
            return line.Length * Clock.SecondsPerCharacter +
                          Clock.SecondsBetweenLines;
        }

        public static int GetNodeDuration(DialogueEntry dialogueEntry)
        {
            var time = 0;
            var timespan = GetTimespan(dialogueEntry);
            var t = dialogueEntry.Timespan();
            
            
            if (timespan != -1)
            {
                time += timespan;
            }

            if (dialogueEntry.Sequence.Contains("BlackOut"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"BlackOut\(([^)]*)\)";
                
                foreach (Match match in Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                    
                }

                var blackoutTime = 0;
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) blackoutTime = secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) blackoutTime = secondsFromLua;
                
                if (extractedContents.Count > 1)
                {
                    switch (extractedContents[1])
                    {
                        case "seconds":
                            time += blackoutTime;
                            break;
                        case "minutes":
                            time += blackoutTime * 60;
                            break;
                        case "hours":
                            time += blackoutTime * 3600;
                            break;
                    }
                    
                }
                
                else blackoutTime *= 60;
                
                time += blackoutTime;

            }
           
            
            if (dialogueEntry.Sequence.Contains("AddSeconds"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"AddSeconds\(([^)]*)\)";
                
                foreach (Match match in Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                }
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) time += secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) time += secondsFromLua;
            }
            
            if (dialogueEntry.Sequence.Contains("AddMinutes"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"AddMinutes\(([^)]*)\)";
                
                foreach (Match match in Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                }
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) time += secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) time += secondsFromLua;
            }
            
            
            if (dialogueEntry.Sequence.Contains("SetTime"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"SetTime\(([^)]*)\)";
                
                foreach (Match match in Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                }
                
                var currentTime = Clock.CurrentTimeRaw;
                
                
                
                var setTime = Clock.ToSeconds(extractedContents[0]);
                var timeDifference = setTime - (currentTime + time);
                
                time += timeDifference;

            }
            
            if (dialogueEntry.userScript.Contains("SetQuestState") && (dialogueEntry.userScript.Contains("success") || dialogueEntry.userScript.Contains("failure")))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"SetQuestState\(([^)]*)\)";
                
                foreach (Match match in Regex.Matches(dialogueEntry.userScript, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim().Replace("\"", ""));
                }
                
                var questName = extractedContents[0].Split(",")[0];
                var quest = GetQuestByName(questName);

                if (quest == null) Debug.LogError("Couldn't find quest " + questName);
                else time += GetQuestDuration(quest);
            }
            
           // if (dialogueEntry.Visited()) return 0;
        
            return time > 0 ? time : GetLineAutoDuration(dialogueEntry.currentDialogueText);
        }

        public static int GetNodeDuration(int conversationID, int nodeID)
        {
            var node = GetDialogueEntryByID(conversationID, nodeID);
            return GetNodeDuration(node);
        }
    }
}