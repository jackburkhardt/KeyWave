using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class PointDistributionViewer : EditorWindow
    {
        [MenuItem("Tools/Perils and Pitfalls/Point Distribution Viewer")]
        private static void ShowWindow()
        {
            var window = GetWindow<PointDistributionViewer>();
            window.titleContent = new GUIContent("Point Distribution Viewer");
            window.Show();
        }
        
        private DialogueDatabase selectedDB;
        private Vector2 scrollPos;
        private bool showWellness;
        private bool showBusiness;
        private bool showSavvy;
        private bool showData;

        private void OnGUI()
        {
            
            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false, GUILayout.Width(400)) as DialogueDatabase;
            
            if (GUILayout.Button("Search", GUILayout.Width(50)))
            {
                if (selectedDB == null)
                {
                    Debug.LogError("Point Distribution Viewer: No database selected!");
                }
                else
                {
                    FindPoints(selectedDB);
                    showData = true;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            if (!showData) return;
                
            EditorGUILayout.LabelField($"Total wellness points: {wellnessTotal} ({(wellnessTotal/(float)totalPoints):P})");
            EditorGUILayout.LabelField($"Total business points: {businessTotal} ({(businessTotal/(float)totalPoints):P})");
            EditorGUILayout.LabelField($"Total savvy points: {savvyTotal} ({(savvyTotal/(float)totalPoints):P})");
                
            EditorGUILayout.Space(20);
                
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            showWellness = EditorGUILayout.Foldout(showWellness, $"Show Wellness Entries ({wellnessEntries.Count})");
            if (showWellness)
            {
                foreach (var entry in wellnessEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.subtitleText} ({entry.Value} points)", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }
                    EditorGUILayout.EndHorizontal();                    }
            }
                
            showBusiness = EditorGUILayout.Foldout(showBusiness, $"Show Business Entries ({businessEntries.Count})");
            if (showBusiness)
            {
                foreach (var entry in businessEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.subtitleText} ({entry.Value} points)", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }
                    EditorGUILayout.EndHorizontal();                    }
            }
                
            showSavvy = EditorGUILayout.Foldout(showSavvy, $"Show Savvy Entries ({savvyEntries.Count})");
            if (showSavvy)
            {
                foreach (var entry in savvyEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.subtitleText} ({entry.Value} points)", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }
                    EditorGUILayout.EndHorizontal();                    }
            }
                
            EditorGUILayout.EndScrollView();

        }
        
        private Dictionary<DialogueEntry, int> wellnessEntries = new();
        private Dictionary<DialogueEntry, int> businessEntries = new();
        private Dictionary<DialogueEntry, int> savvyEntries = new();
        
        private int wellnessTotal;
        private int businessTotal;
        private int savvyTotal;
        private int totalPoints;

        private void FindPoints(DialogueDatabase database)
        {
            ResetPoints();
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    var pointsField = DialogueUtility.GetPointsField(entry);
                    if (pointsField.Type == Points.Type.Null) continue;

                    var points = pointsField.Points;
                    switch (pointsField.Type)
                    {
                        case Points.Type.Wellness:
                            wellnessEntries.Add(entry, points);
                            wellnessTotal += points;
                            break;
                        case Points.Type.Business:
                            businessEntries.Add(entry, points);
                            businessTotal += points;
                            break;
                        case Points.Type.Savvy:
                            savvyEntries.Add(entry, points);
                            savvyTotal += points;
                            break;
                    }
                    
                }
            }
            
            totalPoints = wellnessTotal + businessTotal + savvyTotal;
            
            // sort dictionaries on the value (highest to lowest)
            wellnessEntries = wellnessEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            businessEntries = businessEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            savvyEntries = savvyEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        
        private void ResetPoints()
        {
            wellnessEntries.Clear();
            businessEntries.Clear();
            savvyEntries.Clear();
            wellnessTotal = 0;
            businessTotal = 0;
            savvyTotal = 0;
            showData = false;
        }
    }
}