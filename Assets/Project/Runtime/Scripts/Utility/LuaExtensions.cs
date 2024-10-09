#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.Utility
{
    public static class LuaExtensions
    {
        // Start is called before the first frame update
        public static int Timespan(this DialogueEntry dialogueEntry, string field = "Timespan")
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

        public static int Timespan(this Item quest, string field = "Duration")
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
            var simStatus = Lua.Run("return Conversation[" + dialogueEntry.conversationID + "].Dialog[" +
                                    dialogueEntry.id + "].SimStatus").AsString;
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

        public static string SimStatus(this DialogueEntry entry)
        {
            var simStatus = Lua.Run("return Conversation[" + entry.conversationID + "].Dialog[" + entry.id + "].SimStatus").AsString;
            return simStatus;
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

            return isResponseChild && dialogueEntry.GetActor()!.IsPlayer;
        }

        public static bool IsLastNode(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
        {
            database ??= DialogueManager.MasterDatabase;
            return dialogueEntry.outgoingLinks.Count == 0;
        }

        public static Actor? GetActor(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
        {
            database ??= DialogueManager.MasterDatabase;
            return database.GetActor(dialogueEntry.ActorID);
        }

        public static Actor? GetConversant(this DialogueEntry dialogueEntry, DialogueDatabase? database = null)
        {
            database ??= DialogueManager.MasterDatabase;
            return database.GetActor(dialogueEntry.ConversantID);
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

        public static List<Item> GetQuests(this DialogueDatabase database, string? group)
        {
            var quests = database.items.FindAll(i => i.IsItem != true);
            if (group != null) quests = quests.FindAll(i => i.Group == group);
            return quests;
        }

        public static Item? GetQuest(this DialogueDatabase database, string questName)
        {
            return database.items.Find(i => i.Name == questName);
        }

        public static Field? GetField(this Item item, string fieldName)
        {
            return item.fields.Find(f => f.title == fieldName);
        }

        public static DialogueEntry? GetNextDialogueEntry(this DialogueEntry dialogueEntry,
            DialogueDatabase? database = null)
        {
            database ??= DialogueManager.MasterDatabase;
            var nextDialogueEntry = dialogueEntry.outgoingLinks.Count == 1
                ? dialogueEntry.outgoingLinks[0].GetDestinationEntry(database)
                : null;
            return nextDialogueEntry;
        }

        public static Field? GetField(this DialogueEntry dialogueEntry, string fieldName)
        {
            return Field.Lookup(dialogueEntry.fields, fieldName);
        }

        public static bool IsEmpty(this DialogueEntry dialogueEntry)
        {
            return dialogueEntry is { subtitleText: "", currentMenuText: ""};
        }

        public static Lua.Result? GetLuaField(this Item item, string fieldName)
        {
            return DialogueLua.GetItemField(item.Name, fieldName);
        }

        public static bool EvaluateConditions(this DialogueEntry dialogueEntry)
        {
            return Lua.IsTrue(dialogueEntry.conditionsString);
            
            
        }

        public static DialogueEntry? GetDialogueEntry(this Sequencer sequencer)
        {
            if (string.IsNullOrEmpty(sequencer.entrytag)) return null;
            var splitIndex = sequencer.entrytag.LastIndexOf('_');
            var entryID = int.Parse(sequencer.entrytag.Substring(splitIndex + 1));
            var title = sequencer.entrytag.Substring(0, splitIndex).Replace('_', '/');
            var entry = DialogueManager.instance.masterDatabase.GetConversation(title).GetDialogueEntry(entryID);
            return entry;
        }
       
        
    }
    
    public static class KeyWaveExtensions
    {
        public static Item? GetSubconversationQuest(this DialogueEntry dialogueEntry)
        {
            Item quest = null;
            
            if (dialogueEntry != null && dialogueEntry?.GetNextDialogueEntry() != null && dialogueEntry?.GetNextDialogueEntry()
                    ?.GetConversation() != null)
            quest = DialogueManager.instance.masterDatabase.GetItem(dialogueEntry?.GetNextDialogueEntry()
                ?.GetConversation().Title);

            return quest;
        }
        
        public static string GetCondensedQuestName(this Item subconversationQuest, int? characterLimit = null, string? append = null)
        {
            var title = string.Empty;
            if (subconversationQuest.LookupBool("UseDisplayName")) title = QuestLog.GetQuestTitle(subconversationQuest.Name);
            else title = subconversationQuest.Name.Split('/')[^1];
            if (characterLimit != null && title.Length > characterLimit) title = title.Substring(0, (int)characterLimit);
            if (append != null) title += append;
            return title;
        }
    }
    
    public static class BaseExtensions
    {
        
       public static IEnumerable<T> GetChildren<T>(this Transform transform, IEnumerable<T>? exclude, bool? directChildrenOnly = false)
                where T : Component
            {
                var children = transform.GetComponentsInChildren<T>().ToList();
                children.Remove(transform.GetComponent<T>());
                if (exclude != null)
                {
                    foreach (var item in exclude)
                    {
                        children.Remove(item);
                    }
                }

                var directChildren = new List<T>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
            }
            
            public static IEnumerable<T> GetChildren<T>(this Transform transform, T? exclude = null, bool? directChildrenOnly = false)
                where T : Component
            {
                var children = transform.GetComponentsInChildren<T>(true).ToList();
                children.Remove(transform.GetComponent<T>());
                if (exclude != null)
                {
                    children.Remove(exclude);
                }
                
                
                var directChildren = new List<T>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
            }
            
            public static IEnumerable<T> GetChildren<T>(this Transform transform, (T, T)? exclude, bool? directChildrenOnly = false)
                where T : Component
            {
                var children = transform.GetComponentsInChildren<T>(true).ToList();
                children.Remove(transform.GetComponent<T>());
                if (exclude != null)
                {
                    children.Remove(exclude.Value.Item1);
                    children.Remove(exclude.Value.Item2);
                }
                
               
                var directChildren = new List<T>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
            }
            
            public static IEnumerable<T> GetChildren<T>(this Transform transform, (T, T, T)? exclude, bool? directChildrenOnly = false)
                where T : Component
            {
                var children = transform.GetComponentsInChildren<T>(true).ToList();
                children.Remove(transform.GetComponent<T>());
                if (exclude != null)
                {
                    children.Remove(exclude.Value.Item1);
                    children.Remove(exclude.Value.Item2);
                    children.Remove(exclude.Value.Item3);
                }
                
                var directChildren = new List<T>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
            }

       



            public static IEnumerable<Transform> GetChildren(this Transform transform, IEnumerable<Transform>? exclude, bool? directChildrenOnly = false)
            {
                var children = transform.GetComponentsInChildren<Transform>(true).ToList();
                children.Remove(transform.GetComponent<Transform>());
                if (exclude != null)
                {
                    foreach (var item in exclude)
                    {
                        children.Remove(item);
                    }
                }
                
                var directChildren = new List<Transform>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
            }
        
        
            public static IEnumerable<Transform> GetChildren(this Transform transform, Transform? exclude = null, bool? directChildrenOnly = false)  {
                    var children = transform.GetComponentsInChildren<Transform>(true).ToList();
                    children.Remove(transform.GetComponent<Transform>());
                    if (exclude != null)
                    {
                        children.Remove(exclude);
                    }
                    
                    var directChildren = new List<Transform>();

                    if (directChildrenOnly == true)
                    {
                        foreach (var child in children)
                        {
                            if (child.transform.parent == transform)
                            {
                                directChildren.Add(child);
                            }
                        }

                        return directChildren;
                    }

                    return children;
        }
        
        
            public static IEnumerable<Transform> GetChildren(this Transform transform, (Transform, Transform)? exclude, bool? directChildrenOnly = false)  {
                var children = transform.GetComponentsInChildren<Transform>(true).ToList();
                children.Remove(transform.GetComponent<Transform>());
                if (exclude != null)
                {
                    children.Remove(exclude.Value.Item1);
                    children.Remove(exclude.Value.Item2);
                }
                
                var directChildren = new List<Transform>();

                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent == transform)
                        {
                            directChildren.Add(child);
                        }
                    }

                    return directChildren;
                }

                return children;
                
               
        }
        
        
            public static IEnumerable<Transform> GetChildren(this Transform transform, (Transform, Transform, Transform)? exclude, bool? directChildrenOnly = false)  {
                var children = transform.GetComponentsInChildren<Transform>(true).ToList();
                children.Remove(transform.GetComponent<Transform>());
                if (exclude != null)
                {
                    children.Remove(exclude.Value.Item1);
                    children.Remove(exclude.Value.Item2);
                    children.Remove(exclude.Value.Item3);
                }
                
                if (directChildrenOnly == true)
                {
                    foreach (var child in children)
                    {
                        if (child.transform.parent != transform)
                        {
                            children.Remove(child);
                        }
                    }
                }

                return children; 
        }

        public static GameObject? FindGameObject(this Scene scene, string name)
        {
            GameObject foundObject = null;
            
            foreach (var obj in scene.GetRootGameObjects())
            {
                var children = obj.GetComponentsInChildren<Transform>(true);
                foreach (var child in children)
                {
                    if (foundObject != null) break;
                    if (child.name == name)
                    {
                        foundObject = child.gameObject;
                    }
                }
            }

            return foundObject;
        }
            
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        
        
        public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
    
    
}

    


