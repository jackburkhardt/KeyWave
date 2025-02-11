using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using UnityEditor;
using UnityEngine;
using DialogueEntry = PixelCrushers.DialogueSystem.DialogueEntry;
using NaughtyAttributes;

namespace Plugins.Pixel_Crushers.Dialogue_System.Scripts.Editor.Tools.Perils_Pitfalls
{
    public struct SubconversationData
    {
        public string internalName;
        public string displayName;
        public bool useDisplayNameAsMenuText;
        public string menuText;
        public string dialogueText;
        public string questDescription;
        public string defaultReturnLocation;
        public int questTimeAmount;
        public int questTimeType;
        public int points;
        public int type;
        public bool revistable;
        public Conversation relatedConversation;
        public Item relatedQuest;
    }

    public class CreateSubconversation : EditorWindow
    {
        static Conversation _originConversation;
        static DialogueEntry _originEntry;
        string[] questTimeOptions = { "minutes", "hours", "seconds" };
        string[] pointTypeOptions = { "Skills", "Teamwork", "Context","Wellness", "Null" };
        static bool _editMode;
        private static string DefaultInternalName => $"{_originConversation.Title.Replace("/Base", "")}/";

        public static void ShowSubconversationWindow(object targetConv)
        {
            if (targetConv is not Tuple<Conversation, DialogueEntry, bool> targetData) return;
            _originConversation = targetData.Item1;
            _originEntry = targetData.Item2;
            
            var window = GetWindow<CreateSubconversation>();
            window.titleContent = new GUIContent("Enter information"); ;
            _editMode = targetData.Item3;
            //_defaultInternalName = $"{_originConversation.Title.Replace("/Base", "")}/";

            if (_editMode)
            {
                SelectSubconversationForEdit.ShowPopup(_originEntry, _originConversation.id, window);
                return;
            }
            window.Show();
        }

        public void PopulateFieldsForEdit(SubconversationData data)
        {
            _subconvData = data;
            this.Show();
        }

        // this is the default for a new subconversation
        private SubconversationData _subconvData = new()
        {
            internalName = DefaultInternalName,
            useDisplayNameAsMenuText = true
        };

        private void OnGUI()
        {
            _subconvData.internalName = EditorGUILayout.TextField("Internal Name", _subconvData.internalName);
            _subconvData.displayName = EditorGUILayout.TextField("Display Name", _subconvData.displayName);
            _subconvData.useDisplayNameAsMenuText = EditorGUILayout.Toggle("Use Display Name as Menu Text", _subconvData.useDisplayNameAsMenuText);
            _subconvData.questDescription = EditorGUILayout.TextField("Quest Description", _subconvData.questDescription);
            EditorGUILayout.Separator();
            _subconvData.menuText = _subconvData.useDisplayNameAsMenuText ? _subconvData.displayName : EditorGUILayout.TextField("Menu Text", _subconvData.menuText);
            _subconvData.dialogueText = EditorGUILayout.TextField("Dialogue Text", _subconvData.dialogueText);
            
            EditorGUILayout.BeginHorizontal();
            _subconvData.questTimeAmount = EditorGUILayout.IntField("Quest Time", _subconvData.questTimeAmount);
            _subconvData.questTimeType = EditorGUILayout.Popup(_subconvData.questTimeType, questTimeOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _subconvData.points = EditorGUILayout.IntField("Points", _subconvData.points);
            _subconvData.type = EditorGUILayout.Popup(_subconvData.type, pointTypeOptions);
            EditorGUILayout.EndHorizontal();
            _subconvData.defaultReturnLocation = EditorGUILayout.TextField("Default Return Location (ConvoID:EntryID)", _subconvData.defaultReturnLocation);
            _subconvData.revistable = EditorGUILayout.Toggle("Revisitable", _subconvData.revistable);
            
            if (!string.IsNullOrEmpty(_subconvData.internalName) && !_subconvData.internalName.Contains(DefaultInternalName))
            {
                EditorGUILayout.HelpBox("Internal Name does not seem to contain the origin conversation name. Make sure you've written it correctly.",
                    MessageType.Info);
            }
            
            if (_subconvData.relatedQuest == null)
            {
                EditorGUILayout.HelpBox("No related quest found. One will be created with the given data.",
                    MessageType.Info);
            }
            
            if (_subconvData.relatedConversation == null)
            {
                EditorGUILayout.HelpBox("No related conversation found. One will be created with the given data.",
                    MessageType.Info);
            }
            
            if (string.IsNullOrEmpty(_subconvData.internalName))
            {
                EditorGUILayout.HelpBox("Internal Name cannot be empty. This name is shared by the node, quest, and conversation created with this tool.", MessageType.Error);
            } else if (!string.IsNullOrEmpty(_subconvData.defaultReturnLocation) && !Regex.IsMatch(_subconvData.defaultReturnLocation, @"[0-9]+:[0-9]+"))
            {
                EditorGUILayout.HelpBox("Return Location must be in the format 'ConversationID:EntryID'.",
                    MessageType.Error);
            }
            
            else if (_subconvData.internalName == DefaultInternalName)
            {
                EditorGUILayout.HelpBox("Internal Name must be unique. Please change the name to something other than the default.",
                    MessageType.Error);
            }
            else {
                if (_editMode)
                {
                    if (GUILayout.Button("Save Changes")) Create();
                }
                else
                {
                    if (GUILayout.Button("Create")) Create();
                }
            }
        }
        
        private void Create()
        {
            var db = DialogueEditorWindow.GetCurrentlyEditedDatabase();
            var template = Template.FromDefault();
            CreateQuest(db, template);
            int newConvoId = CreateConversation(db, template);
            CreateNewNode(db, template, newConvoId);
            
            DialogueEditorWindow.instance.SetDatabaseDirty($"Created a subconversation. ({_subconvData.internalName})");
            Close();
        }

        private void CreateQuest(DialogueDatabase db, Template tp)
        {
            var questExists = _subconvData.relatedQuest != null;
            var item = _subconvData.relatedQuest ?? tp.CreateQuest(tp.GetNextQuestID(db), _subconvData.internalName);
            Field.SetValue(item.fields, "Display Name", _subconvData.displayName);
            Field.SetValue(item.fields, "Description", _subconvData.questDescription);

            if (_subconvData.points != 0)
            {
                Field.SetValue(item.fields, "Points", $"{pointTypeOptions[_subconvData.type]}:{_subconvData.points}", FieldType.Points);
            } 
            
            if (_subconvData.questTimeAmount != 0)
            {
                Field.SetValue(item.fields, "Duration", $"{_subconvData.questTimeAmount}:{questTimeOptions[_subconvData.questTimeType]}", FieldType.Timespan);
            }
            
            // add item or replace existing item
            if (!questExists)
            {
                Field.SetValue(item.fields, "State", "active", FieldType.QuestState);
                db.items.Add(item);
            }
            else
            {
                db.items[db.items.FindIndex(i => i.id == item.id)] = item;
            }
        }

        private int CreateConversation(DialogueDatabase db, Template tp, bool addToDatabase = true)
        {
            // creating the conversation
            bool convoExists = _subconvData.relatedConversation != null;
            var newConvo = _subconvData.relatedConversation ?? tp.CreateConversation(tp.GetNextConversationID(db), _subconvData.internalName);
            newConvo.ActorID = -1;
            newConvo.ConversantID = -1;

            if (!convoExists)
            {
                // all conversations should have a START node
                var startNode = tp.CreateDialogueEntry(0, newConvo.id, "START");
                startNode.ActorID = -1;
                startNode.ConversantID = -1;
                startNode.Sequence = "None()"; // START node usually shouldn't play a sequence.
                Field.SetValue(startNode.fields, "Return Location",
                    !string.IsNullOrEmpty(_subconvData.defaultReturnLocation)
                        ? _subconvData.defaultReturnLocation
                        : $"{_originConversation.id}:{_originEntry.id}");

                newConvo.dialogueEntries.Add(startNode);
            }
            else
            {
                // find start node and update return location
                var startNode = newConvo.dialogueEntries.Find(e => e.id == 0);
                Field.SetValue(startNode.fields, "Return Location",
                    !string.IsNullOrEmpty(_subconvData.defaultReturnLocation)
                        ? _subconvData.defaultReturnLocation
                        : $"{_originConversation.id}:{_originEntry.id}");
            }

            if (convoExists)
            {
                db.conversations[db.conversations.FindIndex(c => c.id == newConvo.id)] = newConvo;
            }
            else
            {
                db.AddConversation(newConvo);
            }
            
            DialogueEditorWindow.instance.RefreshConversation();
            return newConvo.id;
        }

        private void CreateNewNode(DialogueDatabase db, Template tp, int newConvoID)
        {
            // dialogue entry and link to new conversation
            var newNode = _originConversation.dialogueEntries.Find(e => e.Title == _subconvData.internalName);
            var nodeExists = newNode != null;
            
            if (!nodeExists)
            {
                newNode = tp.CreateDialogueEntry(
                    tp.GetNextDialogueEntryID(_originConversation), _originConversation.id, _subconvData.internalName);
                newNode.ActorID = -1;
                newNode.ConversantID = -1;
                newNode.canvasRect.x = _originEntry.canvasRect.x;
                newNode.canvasRect.y = _originEntry.canvasRect.y + 70;
            }
            
            newNode.MenuText = _subconvData.menuText;
            newNode.DialogueText = _subconvData.dialogueText;
            if (string.IsNullOrEmpty(newNode.conditionsString)) newNode.conditionsString = "";
            if (!_subconvData.revistable)
            {
                if (!newNode.conditionsString.Contains(
                        $"CurrentQuestState(\"{_subconvData.internalName}\")==\"active\""))
                {
                    // append the condition to the existing conditions
                    newNode.conditionsString += $"\nCurrentQuestState(\"{_subconvData.internalName}\")==\"active\"";
                }
            }
            else
            {
                if (newNode.conditionsString.Contains(
                    $"CurrentQuestState(\"{_subconvData.internalName}\")==\"active\""))
                {
                    // remove the condition from the existing conditions
                    newNode.conditionsString = newNode.conditionsString.Replace(
                        $"CurrentQuestState(\"{_subconvData.internalName}\")==\"active\"", "");
                }
            }
            
            
            if (!nodeExists)
            {
                var newConvoLink = new Link
                {
                    destinationConversationID = newConvoID,
                    destinationDialogueID = 0,
                    originDialogueID = newNode.id, 
                    originConversationID = _originConversation.id
                };
                newNode.outgoingLinks.Add(newConvoLink);
                _originConversation.dialogueEntries.Add(newNode);
            } else
            {
                _originConversation.dialogueEntries[_originConversation.dialogueEntries.FindIndex(e => e.id == newNode.id)] = newNode;
            }
            
            
            // if this was created as a child of another node, establish that link
            if (_originEntry != null && !_originEntry.outgoingLinks.Exists(l => l.destinationDialogueID == newNode.id))
            {
                var childNodeLink = new Link
                {
                    destinationConversationID = _originConversation.id,
                    destinationDialogueID = newNode.id,
                    originDialogueID = _originEntry.id,
                    originConversationID = _originConversation.id
                };
                
                _originEntry.outgoingLinks.Add(childNodeLink);
                //_originConversation.dialogueEntries.Add(_originNode);
            }
        }
        
    }
}