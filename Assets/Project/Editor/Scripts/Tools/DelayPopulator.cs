using System.Collections.Generic;
using PixelCrushers.DialogueSystem.Wrappers;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class DelayPopulator : EditorWindow
    {
        private List<(string Conversation, int ID)> affectedNodes = new();
        private bool hasRun;
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sets the sequence delay to 0 for all entries with no dialogue text and menu option.");
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false, GUILayout.Width(400)) as DialogueDatabase;

            if (GUILayout.Button("Search", GUILayout.Width(50)))
            {
                if (selectedDB == null)
                {
                    Debug.LogError("Delay Populator: No database selected!");
                }
                else
                {
                    UpdateDelays(selectedDB); 
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

        [MenuItem("Tools/Perils and Pitfalls/Database Editor/Node Delay Populator")]
        private static void ShowWindow()
        {
            var window = GetWindow<DelayPopulator>();
            window.titleContent = new GUIContent("Node Delay Populator");
            window.Show();
        }

        private void UpdateDelays(DialogueDatabase database)
        {
            affectedNodes.Clear();
            hasRun = true;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (string.IsNullOrEmpty(entry.Sequence) && 
                        string.IsNullOrEmpty(entry.MenuText) && 
                        string.IsNullOrEmpty(entry.DialogueText))   
                    {
                        entry.Sequence = "Delay(0)";
                        affectedNodes.Add((conversation.Title, entry.id));
                    }
                }
            }
        }
    }
}