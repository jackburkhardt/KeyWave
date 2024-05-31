#nullable enable
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public static class LuaFields
{
    // Start is called before the first frame update
    public static int Timespan (this DialogueEntry dialogueEntry, string field = "Timespan")
    {
        if (!Field.FieldExists(dialogueEntry.fields, field)) return -1;
        
        var timespanField = Field.Lookup(dialogueEntry.fields, field);
        
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
    
    public static int Timespan (this Item quest, string field = "Duration")
    {
        var durationField = quest.AssignedField(field);
        if (durationField == null) return 0;
        
        var unit = durationField.value.Split(':')[1];
        var questTime = int.Parse(durationField.value.Split(':')[0]);
        var duration = 0;

        if (unit == "seconds") duration = questTime;
        else if (unit == "minutes") duration = questTime * 60;
        else if (unit == "hours") duration = questTime * 3600;
        
        return duration;
    }

    public static Conversation GetConversation(this DialogueEntry dialogueEntry)
    {
        var conversation = DialogueUtility.GetConversationByDialogueEntry(dialogueEntry);
        return conversation;
    }
    
    public static bool Visited(this DialogueEntry dialogueEntry)
    {
        var simStatus = Lua.Run("return Conversation[" + dialogueEntry.conversationID + "].Dialog[" + dialogueEntry.id + "].SimStatus").AsString;
        return simStatus == "Visited";
    }
    
    public static DialogueEntry GetDestinationEntry(this Link link, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
        return database.GetDialogueEntry(link.destinationConversationID, link.destinationDialogueID);
     }
    
    public static DialogueEntry GetOriginEntry(this Link link, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
        return database.GetDialogueEntry(link.originConversationID, link.originDialogueID);
    }
    
    public static List<T> AddUnique<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
        return list;
    }

    public static List<Link> GetIncomingLinks(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;

        var links = new List<Link>();

        foreach (var conversation in database.conversations)
        {
            foreach (var dialogue in conversation.dialogueEntries)
            {
                foreach (var link in dialogue.outgoingLinks)
                {
                    if (link.destinationConversationID == dialogueEntry.conversationID &&
                        link.destinationDialogueID == dialogueEntry.id)
                    {
                        links.AddUnique(link);
                    }
                }
            }
        }
        return links;
    }
    
    public static bool IsResponseChild(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
        
        var isResponseChild = false;
        foreach (var link in dialogueEntry.GetIncomingLinks(database))
        {
            if (link.GetOriginEntry(database).IsResponseRoot(database)) isResponseChild = true;
            
        }
        return isResponseChild;
    }
    
    public static Actor? GetActor(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
        return database.GetActor(dialogueEntry.ActorID);
    }
    
    public static bool IsResponseRoot(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
      
        var actor = (dialogueEntry.GetActor(database));

        var isActorPlayer = actor != null && (Field.FieldExists(actor.fields, "IsPlayer") &&
                                              Field.LookupBool(actor.fields, "IsPlayer"));
        
        var doesAnyMenuTextForceResponse = dialogueEntry.outgoingLinks.Exists(link =>
        {
            var destinationEntry = link.GetDestinationEntry(database);
            return destinationEntry != null && destinationEntry.currentMenuText.Contains("[F]");
        });

       return (isActorPlayer && dialogueEntry.outgoingLinks.Count > 1 || doesAnyMenuTextForceResponse);
    }
    
    public static QuestState GetQuestState(this Item quest)
    {
        if (quest.IsItem) return QuestState.Unassigned;
        var questState = QuestLog.GetQuestState(quest.Name);
        return questState;
    }
    
    public static DialogueEntry? GetNextDialogueEntry(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
    {
        database ??= DialogueManager.MasterDatabase;
        var nextDialogueEntry = dialogueEntry.outgoingLinks.Count == 1
            ? dialogueEntry.outgoingLinks[0].GetDestinationEntry(database)
            : null;
        return nextDialogueEntry;
    }

    public static Item? GetSubconversationQuest(this DialogueEntry dialogueEntry)
    {
        var quest = DialogueManager.instance.masterDatabase.GetItem(dialogueEntry?.GetNextDialogueEntry()?.GetConversation().Title);

        return quest;
    }
    
    public static Field? GetField(this DialogueEntry dialogueEntry, string fieldName)
    {
        return Field.Lookup(dialogueEntry.fields, fieldName);
    }

    public static bool IsEmpty(this DialogueEntry dialogueEntry)
    {

        return dialogueEntry is { subtitleText: "", currentMenuText: "" };
    }
    
    public static Lua.Result? GetLuaField(this Item item, string fieldName)
    {
        return DialogueLua.GetItemField(item.Name, fieldName);
    }
}
