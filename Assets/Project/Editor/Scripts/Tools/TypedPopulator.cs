using System.Collections.Generic;
using PixelCrushers.DialogueSystem.Wrappers;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class TypedPopulator : EditorWindow
    {
        [MenuItem("Tools/Perils and Pitfalls/Database Editor/Node Typed Populator")]
        private static void ShowWindow()
        {
            var window = GetWindow<TypedPopulator>();
            window.titleContent = new GUIContent("Node Typed Populator");
            window.Show();
        }

        private DialogueDatabase selectedDB;
        private Vector2 scrollPos;
        private bool hasRun;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sets the sequence to {{typed}} for all entries preceding an entry with no dialogue text and menu option.");
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false, GUILayout.Width(400)) as DialogueDatabase;

            if (GUILayout.Button("Search", GUILayout.Width(50)))
            {
                if (selectedDB == null)
                {
                    Debug.LogError("Typed Populator: No database selected!");
                }
                else
                {
                    UpdateTypedSequence(selectedDB); 
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
        
        private List<(string Conversation, int ID)> affectedNodes = new();
        
        private void UpdateTypedSequence(DialogueDatabase database)
        {
            affectedNodes.Clear();
            hasRun = true;
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry.outgoingLinks.Count != 1) continue;

                    if (entry.IsResponseChild(database)) continue;
                    
                    if (string.IsNullOrEmpty(entry.MenuText) && 
                        string.IsNullOrEmpty(entry.DialogueText)) continue;
                    
                    var nextEntry = entry.outgoingLinks[0].GetDestinationEntry(database);
                    
                    if (nextEntry == null || nextEntry.isRoot) continue;
                    
                    if (string.IsNullOrEmpty(nextEntry.MenuText) && 
                        string.IsNullOrEmpty(nextEntry.DialogueText) || nextEntry.Sequence.Contains("Delay(0)"))   
                    {
                        entry.Sequence = "{{typed}}";
                        affectedNodes.Add((conversation.Title, entry.id));
                    }
                }
            }
        }
    }
}