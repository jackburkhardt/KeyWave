using System;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using UnityEditor;
using UnityEngine;
using DialogueEntry = PixelCrushers.DialogueSystem.DialogueEntry;
using NaughtyAttributes;

namespace Plugins.Pixel_Crushers.Dialogue_System.Scripts.Editor.Tools.Perils_Pitfalls
{
    public class CreateSubcoversation : EditorWindow
    {
        static Conversation _originConversation;
        static DialogueEntry _originEntry;
        string[] questTimeOptions = { "minutes", "hours", "seconds" };
        string[] pointTypeOptions = { "Wellness", "Savvy", "Business", "Null" };
        
        public static void ShowSubconversationWindow(object targetConv)
        {
            if (targetConv is not Tuple<Conversation, DialogueEntry> targetData) return;
            _originConversation = targetData.Item1;
            _originEntry = targetData.Item2;
            
            var window = GetWindow<CreateSubcoversation>();
            window.titleContent = new GUIContent("Enter information");
            window.Show();
        }
        
        string _internalName;
        string _displayName;
        private bool _useDisplayNameAsMenuText = true;
        private string _menuText;
        string _dialogueText;
        private string _questDescription;
        private string _defaultReturnLocation;
        int _questTimeAmount;
        int _questTimeType;
        int _points;
        int _type;
        private bool _revistable;

        private void OnGUI()
        {
            _internalName = EditorGUILayout.TextField("Internal Name", _internalName);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);
            _useDisplayNameAsMenuText = EditorGUILayout.Toggle("Use Display Name as Menu Text", _useDisplayNameAsMenuText);
            _questDescription = EditorGUILayout.TextField("Quest Description", _questDescription);
            EditorGUILayout.Separator();
            _menuText = _useDisplayNameAsMenuText ? _displayName : EditorGUILayout.TextField("Menu Text", _menuText);
            _dialogueText = EditorGUILayout.TextField("Dialogue Text", _dialogueText);
            
            EditorGUILayout.BeginHorizontal();
                _questTimeAmount = EditorGUILayout.IntField("Quest Time", _questTimeAmount);
                _questTimeType = EditorGUILayout.Popup(_questTimeType, questTimeOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                _points = EditorGUILayout.IntField("Points", _points);
                _type = EditorGUILayout.Popup(_type, pointTypeOptions);
            EditorGUILayout.EndHorizontal();
            _defaultReturnLocation = EditorGUILayout.TextField("Default Return Location (ConvoID:EntryID)", _defaultReturnLocation);
            _revistable = EditorGUILayout.Toggle("Revisitable", _revistable);
            
            if (string.IsNullOrEmpty(_internalName))
            {
                EditorGUILayout.HelpBox("Internal Name cannot be empty. This name is shared by the node, quest, and conversation created with this tool.", MessageType.Error);
            } else if (!string.IsNullOrEmpty(_defaultReturnLocation) && !Regex.IsMatch(_defaultReturnLocation, @"[0-9]+:[0-9]+"))
            {
                EditorGUILayout.HelpBox("Return Location must be in the format 'ConversationID:EntryID'.",
                    MessageType.Error);
            }
            else if (GUILayout.Button("Create"))
            {
                Create();
            }
        }

        private void Create()
        {
            var db = DialogueEditorWindow.GetCurrentlyEditedDatabase();
            var template = Template.FromDefault();
            
            // creating the quest
            var item = template.CreateQuest(template.GetNextQuestID(db), _internalName);
            Field.SetValue(item.fields, "State", "active", FieldType.QuestState);
            Field.SetValue(item.fields, "Display Name", _displayName);
            Field.SetValue(item.fields, "Description", _questDescription);

            if (_points != 0)
            {
                Field.SetValue(item.fields, "Points", $"{pointTypeOptions[_type]}:{_points}", FieldType.Points);
            } 
            
            if (_questTimeAmount != 0)
            {
                Field.SetValue(item.fields, "Duration", $"{_questTimeAmount}:{questTimeOptions[_questTimeType]}", FieldType.Timespan);
            }
            db.items.Add(item);
            
            // creating the conversation
            var newConvo = template.CreateConversation(template.GetNextConversationID(db), _internalName);
            newConvo.ActorID = 1;
            newConvo.ConversantID = -1;
            
            // all conversations should have a START node
            var startNode = template.CreateDialogueEntry(0, newConvo.id, "START");
            startNode.ActorID = 1;
            startNode.ConversantID = -1;
            startNode.Sequence = "None()"; // START node usually shouldn't play a sequence.
            Field.SetValue(startNode.fields, "Return Location",
                !string.IsNullOrEmpty(_defaultReturnLocation)
                    ? _defaultReturnLocation
                    : $"{_originConversation.id}:{_originEntry.id}");

            newConvo.dialogueEntries.Add(startNode);
            
            db.AddConversation(newConvo);
            
            DialogueEditorWindow.instance.RefreshConversation();

            // dialogue entry and link to new conversation
            var newNode = template.CreateDialogueEntry(
                template.GetNextDialogueEntryID(_originConversation), _originConversation.id, _internalName);
            newNode.ActorID = 1;
            newNode.ConversantID = -1;
            newNode.MenuText = _menuText;
            newNode.DialogueText = _dialogueText;
            newNode.canvasRect.x = _originEntry.canvasRect.x;
            newNode.canvasRect.y = _originEntry.canvasRect.y + 70;
            if (!_revistable)
            {
                newNode.conditionsString = $"CurrentQuestState(\"{_internalName}\") == \"active\"";
            }
            
            var newConvoLink = new Link
            {
                destinationConversationID = newConvo.id,
                destinationDialogueID = 0,
                originDialogueID = newNode.id, 
                originConversationID = _originConversation.id
            };
            
            newNode.outgoingLinks.Add(newConvoLink);
            _originConversation.dialogueEntries.Add(newNode);
            
            // if this was created as a child of another node, establish that link
            if (_originEntry != null)
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
            
            
            DialogueEditorWindow.instance.SetDatabaseDirty($"Created a subconversation. ({_internalName})");
            
            Close();
        }
    }
}