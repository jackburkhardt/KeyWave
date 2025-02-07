using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class PointDistributionViewer : EditorWindow
    {
        private Dictionary<Item, int> SkillsEntries = new();
        private int SkillsTotal;
        private Dictionary<Item, int> TeamworkEntries = new();
        private int TeamworkTotal;
        private Dictionary<Item, int> ContextEntries = new();
        private int ContextTotal;
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;
        private bool showCred;
        private bool showData;
        private bool showTeamwork;
        private bool showWellness;
        private bool showCommit;
        private int totalPoints;
        private bool includeZeroPointEntries;

        private Dictionary<Item, int> wellnessEntries = new();

        private int wellnessTotal;

        private string wellnessMaxVar = "points.wellness.max";
        private string TeamworkMaxVar = "points.teamwork.max";
        private string SkillsMaxVar = "points.skills.max";
        private string ContextMaxVar = "points.context.max";

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
                $"Total Skills points: {SkillsTotal} ({(SkillsTotal / (float)totalPoints):P})");
            EditorGUILayout.LabelField(
                $"Total Teamwork points: {TeamworkTotal} ({(TeamworkTotal / (float)totalPoints):P})");
            EditorGUILayout.LabelField(
                $"Total Context points: {ContextTotal} ({(ContextTotal / (float)totalPoints):P})");

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

            showCred = EditorGUILayout.Foldout(showCred, $"Show Skills Entries ({SkillsEntries.Count})");
            if (showCred)
            {
                foreach (var entry in SkillsEntries)
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

            showTeamwork =
                EditorGUILayout.Foldout(showTeamwork, $"Show Teamwork Entries ({TeamworkEntries.Count})");
            if (showTeamwork)
            {
                foreach (var entry in TeamworkEntries)
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

            showCommit = EditorGUILayout.Foldout(showCommit, $"Show Context Entries ({ContextEntries.Count})");
            if (showCommit)
            {
                foreach (var entry in ContextEntries)
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
            
            TeamworkMaxVar = EditorGUILayout.TextField("Teamwork", TeamworkMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == TeamworkMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
           
            SkillsMaxVar = EditorGUILayout.TextField("Skills", SkillsMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == SkillsMaxVar) == null)
            {
                EditorGUILayout.HelpBox("Variable not found in database!", MessageType.Error);
            }
            
            ContextMaxVar = EditorGUILayout.TextField("Context", ContextMaxVar);
            
            if (selectedDB.variables.Find(p => p.Name == ContextMaxVar) == null)
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
                    if (points <= 0 && !includeZeroPointEntries) continue;
                    
                    switch (field.Type)
                    {
                        case Points.Type.Wellness:
                            wellnessEntries.Add(quest, points);
                            wellnessTotal += points;
                            break;
                        case Points.Type.Skills:
                            SkillsEntries.Add(quest, points);
                            SkillsTotal += points;
                            break;
                        case Points.Type.Teamwork:
                            TeamworkEntries.Add(quest, points);
                            TeamworkTotal += points;
                            break;
                        case Points.Type.Context:
                            ContextEntries.Add(quest, points);
                            ContextTotal += points;
                            break;
                    }
                }
                
            }

            totalPoints = wellnessTotal + SkillsTotal + TeamworkTotal + ContextTotal;

            // sort dictionaries on the value (highest to lowest)
            wellnessEntries = wellnessEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            SkillsEntries = SkillsEntries.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
            TeamworkEntries =
                TeamworkEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            ContextEntries =
                ContextEntries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private void ResetPoints()
        {
            wellnessEntries.Clear();
            SkillsEntries.Clear();
            TeamworkEntries.Clear();
            ContextEntries.Clear();
            wellnessTotal = 0;
            SkillsTotal = 0;
            TeamworkTotal = 0;
            ContextTotal = 0;
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
                else if (v.Name == TeamworkMaxVar)
                {
                    v.InitialFloatValue = SkillsTotal;
                }
                else if (v.Name == SkillsMaxVar)
                {
                    v.InitialFloatValue = TeamworkTotal;
                }
                else if (v.Name == ContextMaxVar)
                {
                    v.InitialFloatValue = ContextTotal;
                }
            });
        }
    }
}