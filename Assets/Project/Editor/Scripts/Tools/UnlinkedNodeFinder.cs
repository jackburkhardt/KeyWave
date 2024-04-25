using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class UnlinkedNodeFinder : EditorWindow
    {
        [MenuItem("Tools/Perils and Pitfalls/Unlinked Node Finder")]
        private static void ShowWindow()
        {
            var window = GetWindow<UnlinkedNodeFinder>();
            window.titleContent = new GUIContent("Unlinked Node Finder");
            window.Show();
        }

        private DialogueDatabase selectedDB;
        private Vector2 scrollPos;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Searches for all nodes with no outgoing links or scene events, which may lead to a softlock.");
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false, GUILayout.Width(400)) as DialogueDatabase;

            if (GUILayout.Button("Search", GUILayout.Width(50)))
            {
                if (selectedDB == null)
                {
                    Debug.LogError("Unlinked Node Finder: No database selected!");
                }
                else
                {
                    foundNodes = SearchForMissingLinks(selectedDB);   
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            
            if (foundNodes.Count > 0)
            {
                EditorGUILayout.LabelField("Unlinked nodes:", new GUIStyle { fontStyle = FontStyle.Bold });
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var item in foundNodes)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{item.Converstion} ({item.Entry.subtitleText})", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, item.Entry.conversationID, item.Entry.id);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private List<(string Converstion, DialogueEntry Entry)> foundNodes = new();

        public static List<(string Converstion, DialogueEntry Entry)> SearchForMissingLinks(DialogueDatabase database)
        {
            if (database == null) return new List<(string Converstion, DialogueEntry Entry)>();
            
            var unlinkedNodes = new List<(string Converstion, DialogueEntry Entry)>();
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.outgoingLinks.Count == 0 && string.IsNullOrEmpty(entry.sceneEventGuid))
                    {
                        //Debug.Log($"Unlinked node found: {conversation.Title} ({entry.subtitleText}) with event {entry.sceneEventGuid}");
                        unlinkedNodes.Add((conversation.Title, entry));
                    }
                }
            }

            return unlinkedNodes;
        }
    }
}