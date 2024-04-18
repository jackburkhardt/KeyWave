using System;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class FixLinklessNodes : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>("Assets/Dialogue Database.asset");
            var foundNodes = UnlinkedNodeFinder.SearchForMissingLinks(database);

            foreach (var node in foundNodes)
            {
                var foundQuest = database.items.Find((item => item.Name == node.Converstion));
                if (foundQuest != null)
                {
                    node.Entry.userScript = $"SetQuestState(\"{foundQuest.Name}\", \"success\")";
                }
                
                var convo = database.GetConversation(node.Converstion);
                
                // get the return location from starting node and create a link
                var startingNode = convo.GetFirstDialogueEntry();
                var returnLocString = Field.LookupValue(startingNode.fields, "Return Location");
                if (returnLocString != null)
                {
                    // format is convId:entryId
                    var returnLoc = returnLocString.Split(':');
                    var returnLink = new Link
                    {
                        destinationConversationID = int.Parse(returnLoc[0]),
                        destinationDialogueID = int.Parse(returnLoc[1]),
                        originConversationID = node.Entry.conversationID,
                        originDialogueID = node.Entry.id,
                        priority = ConditionPriority.Low
                    };
                    node.Entry.outgoingLinks.Add(returnLink);
                }
                else
                {
                    Debug.LogWarning($"The conversation {node.Converstion} has no return location set! " +
                                   $"Terminating node {node.Entry.id} has no outgoing links and may softlock.");
                }

                // replace the entry in the conversation
                var entry = convo.GetDialogueEntry(node.Entry.id);
                convo.dialogueEntries.Remove(entry);
                convo.dialogueEntries.Add(node.Entry);
                
                EditorUtility.SetDirty(database);
                
            }
        }
    }
}