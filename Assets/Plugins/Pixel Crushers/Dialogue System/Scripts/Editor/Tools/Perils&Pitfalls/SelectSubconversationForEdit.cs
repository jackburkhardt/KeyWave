using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using UnityEditor;
using UnityEngine;

namespace Plugins.Pixel_Crushers.Dialogue_System.Scripts.Editor.Tools.Perils_Pitfalls
{
    public class SelectSubconversationForEdit : EditorWindow
    {
        static List<Conversation> potentialSubconversations = new ();
        static CreateSubconversation originWindow;
        private static DialogueEntry selectedNode;
        
        public static void ShowPopup(DialogueEntry node, int thisConvoId, CreateSubconversation originW)
        {
            potentialSubconversations.Clear();
            var db = DialogueEditorWindow.GetCurrentlyEditedDatabase();
            foreach (var link in node.outgoingLinks)
            {
                if (link.destinationConversationID == thisConvoId) continue;
                var subconv = db.GetConversation(link.destinationConversationID);
                potentialSubconversations.Add(subconv);
            }
            
            originWindow = originW;
            selectedNode = node;
            var window = GetWindow<SelectSubconversationForEdit>();
            window.titleContent = new GUIContent("Select Subconversation to Edit");
            window.ShowPopup();
        }
        
        Conversation selectedSubconversation;
        int chosenSubconversationIndex;
        private void OnGUI()
        {
            if (potentialSubconversations.Count == 0)
            {
                EditorGUILayout.LabelField("No valid subconversations found. The tool will attempt to find one or create a new one.");
            }
            chosenSubconversationIndex = EditorGUILayout.Popup("Select Subconversation", chosenSubconversationIndex, potentialSubconversations.ConvertAll(conv => conv.Title).ToArray());
            if (GUILayout.Button("Continue"))
            {
                selectedSubconversation = potentialSubconversations.Count > 0 ? potentialSubconversations[chosenSubconversationIndex] : null;
                PullDataForEdit();
            }
        }

        private void PullDataForEdit()
        {
            var db = DialogueEditorWindow.GetCurrentlyEditedDatabase();
            var subConv = new SubconversationData();

            var foundQuest = db.GetItem(selectedNode.Title);
            subConv.relatedQuest = foundQuest;
            if (subConv.relatedQuest != null)
            {
                string pointField = foundQuest.LookupValue("Points");
                if (pointField != null)
                {
                    subConv.points = int.Parse(pointField.Split(':')[1]);
                    subConv.type = (int)Enum.Parse<PointsType>(pointField.Split(':')[0]);
                }
                
                string durationField = foundQuest.LookupValue("Duration");
                if (durationField != null)
                {
                    subConv.questTimeAmount = int.Parse(durationField.Split(':')[0]);
                    subConv.questTimeType = (int)Enum.Parse<CustomFieldType_TimespanType.TimespanType>(durationField.Split(':')[1]);
                }
                
                subConv.questDescription = foundQuest.LookupValue("Description");
                subConv.displayName = foundQuest.LookupValue("Display Name");
            }

            subConv.relatedConversation = selectedSubconversation;
            if (selectedSubconversation != null)
            {
                var startNode = selectedSubconversation.dialogueEntries.Find(e => e.id == 0);
                if (startNode == null)
                {
                    Debug.LogError($"Subconversation Editor: Was given a conversation with no start node. How did you manage that? Conversation: {selectedSubconversation.Title}");
                }
                else
                {
                    subConv.defaultReturnLocation = Field.LookupValue(startNode.fields, "Return Location") ?? "";
                }
            }

            subConv.dialogueText = selectedNode.DialogueText;
            subConv.revistable =
                selectedNode.conditionsString.Contains($"CurrentQuestState(\"{subConv.internalName}\") == \"active\"");
            subConv.internalName = selectedNode.Title;
            subConv.menuText = selectedNode.MenuText;
            subConv.useDisplayNameAsMenuText = subConv.menuText == subConv.displayName;

            originWindow.PopulateFieldsForEdit(subConv);
            Close();
        }
        
        
    }
}