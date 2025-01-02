using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.DialogueEditor;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class PointDistributionViewer : EditorWindow
    {
        private Dictionary<Item, int> credibilityEntries = new();
        private int credibilityTotal;
        private Dictionary<Item, int> EngagementEntries = new();
        private int EngagementTotal;
        private Dictionary<Item, int> commitmentEntries = new();
        private int commitmentTotal;
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;
        private bool showCred;
        private bool showData;
        private bool showEngagement;
        private bool showWellness;
        private bool showCommit;
        private int totalPoints;
        private bool includeZeroPointEntries;

        private Dictionary<Item, int> wellnessEntries = new();

        private int wellnessTotal;

        private string wellnessMaxVar = "points.wellnessMAX";
        private string engagementMaxVar = "points.engagementMAX";
        private string credibilityMaxVar = "points.credibilityMAX";
        private string commitmentMaxVar = "points.commitmentMAX";

        private void OnGUI()
        {

            EditorGUILayout.BeginHorizontal();
            selectedDB = EditorGUILayout.ObjectField("Dialogue Database", selectedDB, typeof(DialogueDatabase), false,
                GUILayout.Width(400)) as DialogueDatabase;

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

            var defaultWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            includeZeroPointEntries =
                EditorGUILayout.Toggle("Include entries with zero points", includeZeroPointEntries);
            EditorGUIUtility.labelWidth = defaultWidth;

            EditorGUILayout.Space(20);

            if (!showData) return;

            EditorGUILayout.LabelField(
                $"Total wellness points: {wellnessTotal} ({(wellnessTotal / (float)totalPoints):P})");
            EditorGUILayout.LabelField(
                $"Total credibility points: {credibilityTotal} ({(credibilityTotal / (float)totalPoints):P})");
            EditorGUILayout.LabelField(
                $"Total engagement points: {EngagementTotal} ({(EngagementTotal / (float)totalPoints):P})");
            EditorGUILayout.LabelField(
                $"Total commitment points: {commitmentTotal} ({(commitmentTotal / (float)totalPoints):P})");

            EditorGUILayout.Space(20);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            showWellness = EditorGUILayout.Foldout(showWellness, $"Show Wellness Entries ({wellnessEntries.Count})");
            if (showWellness)
            {
                foreach (var entry in wellnessEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.Name} ({entry.Value} points)",
                        GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                       // DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            showCred = EditorGUILayout.Foldout(showCred, $"Show Credibility Entries ({credibilityEntries.Count})");
            if (showCred)
            {
                foreach (var entry in credibilityEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.Name} ({entry.Value} points)",
                        GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        //DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            showEngagement =
                EditorGUILayout.Foldout(showEngagement, $"Show Engagement Entries ({EngagementEntries.Count})");
            if (showEngagement)
            {
                foreach (var entry in EngagementEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.Name} ({entry.Value} points)",
                        GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        //DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.Key.conversationID, entry.Key.id);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            showCommit = EditorGUILayout.Foldout(showCommit, $"Show Commitment Entries ({commitmentEntries.Count})");
            if (showCommit)
            {
                foreach (var entry in commitmentEntries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{entry.Key.Name} ({entry.Value} points)",
                        GUILayout.Width(400));
                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                    {
                        //DialogueEditorWindow.OpenDialogueEntry(selectedDB, entry.id, entry.Key.id);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            

            
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space(30);
            

            EditorGUILayout.LabelField("Max Points Variables", EditorStyles.boldLabel);
            
            wellnessMaxVar = EditorGUILayout.TextField("Wellness", wellnessMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == wellnessMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
            
            engagementMaxVar = EditorGUILayout.TextField("Engagement", engagementMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == engagementMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
           
            credibilityMaxVar = EditorGUILayout.TextField("Credibility", credibilityMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == credibilityMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
            
            commitmentMaxVar = EditorGUILayout.TextField("Commitment", commitmentMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == commitmentMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
            
            if (GUILayout.Button("Set Variables", GUILayout.MaxWidth(100)))
            {
                SetMaxPoints(selectedDB);
            }
            
            EditorGUILayout.Space(20);
            
            EditorGUILayout.EndVertical();
            
            
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
            foreach (var quest in database.items)
            {
                if (quest.IsItem) continue;
                
                var pointsField = QuestUtility.GetPoints(quest, database);
                if (pointsField == null || pointsField.Length == 0) continue;

                foreach (Points.PointsField field in pointsField)
                {
                    var points = field.Points;
                    if (points == 0 && !includeZeroPointEntries) continue;
                    
                    switch (field.Type)
                    {
                        case Points.Type.Wellness:
                            wellnessEntries.Add(quest, points);
                            wellnessTotal += points;
                            break;
                        case Points.Type.Credibility:
                            credibilityEntries.Add(quest, points);
                            credibilityTotal += points;
                            break;
                        case Points.Type.Engagement:
                            EngagementEntries.Add(quest, points);
                            EngagementTotal += points;
                            break;
                        case Points.Type.Commitment:
                            commitmentEntries.Add(quest, points);
                            commitmentTotal += points;
                            break;
                    }
                }
                
            }

            totalPoints = wellnessTotal + credibilityTotal + EngagementTotal + commitmentTotal;

            // sort dictionaries on the value (highest to lowest)
            wellnessEntries = wellnessEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            credibilityEntries = credibilityEntries.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
            EngagementEntries =
                EngagementEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            commitmentEntries =
                commitmentEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
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

        private void SetMaxPoints(DialogueDatabase database)
        {
            database.variables.FindAll(v => v.Name.StartsWith("points.")).ForEach(v =>
            {
                if (v.Name == wellnessMaxVar)
                {
                    v.InitialFloatValue = wellnessTotal;
                }
                else if (v.Name == engagementMaxVar)
                {
                    v.InitialFloatValue = credibilityTotal;
                }
                else if (v.Name == credibilityMaxVar)
                {
                    v.InitialFloatValue = EngagementTotal;
                }
                else if (v.Name == commitmentMaxVar)
                {
                    v.InitialFloatValue = commitmentTotal;
                }
            });
        }
    }
}