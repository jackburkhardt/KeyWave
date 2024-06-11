using System.Collections.Generic;
using PixelCrushers.DialogueSystem.Wrappers;
using Project.Runtime.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class ClearResponses : EditorWindow
    {
        private List<(string Conversation, int ID)> affectedNodes = new();
        private bool hasRun;
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Clears the sequence field for all nodes that are responses.");
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false, GUILayout.Width(400)) as DialogueDatabase;

            if (GUILayout.Button("Search", GUILayout.Width(50)))
            {
                if (selectedDB == null)
                {
                    Debug.LogError("Clear Responses: No database selected!");
                }
                else
                {
                    UpdateResponses(selectedDB); 
                }
            }
            EditorGUILayout.EndHorizontal();

            if (hasRun)
            {
                EditorGUILayout.LabelField($"Operation affected {affectedNodes.Count} nodes.", new GUIStyle { fontStyle = FontStyle.Bold });
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var (conversation, id) in affectedNodes)
                {
                    EditorGUILayout.LabelField($"{conversation} ({id})");
                }
                EditorGUILayout.EndScrollView();
            }
        }

        [MenuItem("Tools/Perils and Pitfalls/Database Editor/Clear Responses")]
        private static void ShowWindow()
        {
            var window = GetWindow<ClearResponses>();
            window.titleContent = new GUIContent("Clear Responses");
            window.Show();
        }

        private void UpdateResponses(DialogueDatabase database)
        {
            affectedNodes.Clear();
            hasRun = true;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.IsResponseChild(database))
                    {
                        entry.Sequence = "";
                    }
                }
            }
        }
    }
}