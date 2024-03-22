using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

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
    

    public static Color NodeColor(DialogueEntry node)
    {
        var visitedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        var leaveColor = new Color(0.2f, 0.05f, 0.05f, 1f);
        var defaultColor = Location.PlayerLocation.responseMenuButtonColor;
        
        if (node == null) return Color.white;
        if (Field.FieldExists(node.fields, "Visited") && Field.LookupBool(node.fields, "Visited"))
            return Color.Lerp(visitedColor, defaultColor, 0.4f);
        if (node.Title == "Leave") return Color.Lerp(leaveColor, defaultColor, 0.2f);
        return defaultColor;
    }
    
    public struct PointsField
    {
        public Points.Type type;
        public int points;
    }
    
    public static PointsField GetPointsField(DialogueEntry dialogueEntry)
    {
        var field = Field.Lookup(dialogueEntry.fields, "Points");
        if (field == null) return new PointsField {type = Points.Type.Null, points = 0};
        var pointsType = (Points.Type) Enum.Parse(typeof(Points.Type), field.value.Split(':')[0]);
        var pointsValue = int.Parse(field.value.Split(':')[1]);
        return new PointsField {type = pointsType, points = pointsValue};
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
                var entry = DialogueUtility.GetDialogueEntryFromNodeField(field);
                var timeEstimate = DialogueUtility.DurationRangeBetweenNodes(node, entry);
                
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
    
    private static int GetLineAutoDuration(string line)
    {
        if (line == string.Empty) return 0;
        return (line.Length / Clock.TimeScales.SpokenCharactersPerSecond +
                Clock.TimeScales.SecondsBetweenLines) * Clock.TimeScales.GlobalTimeScale;
    }
    
    public static int GetNodeDuration(int conversationID, int nodeID)
    {
        var node = GetDialogueEntryByID(conversationID, nodeID);
       
        var durationField = Field.LookupInt(node.fields, "Duration");
        return durationField == 0 ? GetLineAutoDuration(node.currentDialogueText) : durationField;
    }
    
    

    
}
