using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class PointDistributionViewer : EditorWindow
    {
        private Dictionary<DialogueEntry, int> credibilityEntries = new();
        private int credibilityTotal;
        private Dictionary<DialogueEntry, int> EngagementEntries = new();
        private int EngagementTotal;
        private Dictionary<DialogueEntry, int> commitmentEntries = new();
        private int commitmentTotal;
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;
        private bool showCred;
        private bool showData;
        private bool showEngagement;
        private bool showWellness;
        private bool showCommit;
        private int totalPoints;

        private Dictionary<DialogueEntry, int> wellnessEntries = new();

        private int wellnessTotal;

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
            EditorGUILayout.LabelField($"Total cred points: {credibilityTotal} ({(credibilityTotal/(float)totalPoints):P})");
            EditorGUILayout.LabelField($"Total rapoport points: {EngagementTotal} ({(EngagementTotal/(float)totalPoints):P})");
            EditorGUILayout.LabelField($"Total commitment points: {commitmentTotal} ({(commitmentTotal/(float)totalPoints):P})");
                
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
                
            showCred = EditorGUILayout.Foldout(showCred, $"Show Credibility Entries ({credibilityEntries.Count})");
            if (showCred)
            {
                foreach (var entry in credibilityEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.subtitleText} ({entry.Value} points)", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }
                    EditorGUILayout.EndHorizontal();                    }
            }
                
            showEngagement = EditorGUILayout.Foldout(showEngagement, $"Show Engagement Entries ({EngagementEntries.Count})");
            if (showEngagement)
            {
                foreach (var entry in EngagementEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.subtitleText} ({entry.Value} points)", GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }
                    EditorGUILayout.EndHorizontal();                    }
            }
            
            showCommit = EditorGUILayout.Foldout(showCommit, $"Show Commitment Entries ({commitmentEntries.Count})");
            if (showCommit)
            {
                foreach (var entry in commitmentEntries)
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

        [MenuItem("Tools/Perils and Pitfalls/Viewer/Point Distribution Viewer")]
        private static void ShowWindow()
        {
            var window = GetWindow<PointDistributionViewer>();
            window.titleContent = new GUIContent("Point Distribution Viewer");
            window.Show();
        }

        private void FindPoints(DialogueDatabase database)
        {
            ResetPoints();
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    var pointsField = QuestUtility.GetPoints(entry);
                    if (pointsField.Length == 0) continue;

                    foreach (var field in pointsField)
                    {
                        var points = field.Points;
                        switch (field.Type)
                        {
                            case Points.Type.Wellness:
                                wellnessEntries.Add(entry, points);
                                wellnessTotal += points;
                                break;
                            case Points.Type.Engagement:
                                credibilityEntries.Add(entry, points);
                                credibilityTotal += points;
                                break;
                            case Points.Type.Credibility:
                                EngagementEntries.Add(entry, points);
                                EngagementTotal += points;
                                break;
                            case Points.Type.Commitment:
                                commitmentEntries.Add(entry, points);
                                commitmentTotal += points;
                                break;
                        }
                    }

                }
            }
            
            totalPoints = wellnessTotal + credibilityTotal + EngagementTotal + commitmentTotal;
            
            // sort dictionaries on the value (highest to lowest)
            wellnessEntries = wellnessEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            credibilityEntries = credibilityEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            EngagementEntries = EngagementEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            commitmentEntries = commitmentEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private void ResetPoints()
        {
            wellnessEntries.Clear();
            credibilityEntries.Clear();
            EngagementEntries.Clear();
            commitmentEntries.Clear();
            wellnessTotal = 0;
            credibilityTotal = 0;
            EngagementTotal = 0;
            commitmentTotal = 0;
            showData = false;
        }
    }
}