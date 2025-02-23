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
        
        Dictionary<Item, (bool, int)> points = new Dictionary<Item, (bool, int)>();
        
        private Vector2 scrollPos;

        private DialogueDatabase selectedDB;
        private bool showData;
        private int totalPoints;
        private bool includeZeroPointEntries;

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

            Item pointsCategory;
            List<Item> questsWithMatchingPoints;

            for (int i = 0; i < points.Count(); i++)
            {
                
                pointsCategory = points.ElementAt(i).Key;
                questsWithMatchingPoints = Points.GetAllItemsWithPointsType(pointsCategory, selectedDB, includeZeroPointEntries);
                
                EditorGUILayout.LabelField(
                    $"Total {pointsCategory.Name} points: {pointsCategory.LookupInt("Max Score")} ({(points.ElementAt(i).Key.LookupInt("Max Score") / (float)totalPoints):P})");
            }
            
            
            EditorGUILayout.Space(20);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for ( int i = 0; i < points.Count; i++)
            {
                var element = points.ElementAt(i);
                var item = points.ElementAt(i).Key;
                
                var quests = Points.GetAllItemsWithPointsType( item, selectedDB, false);
                var foldout = EditorGUILayout.Foldout( element.Value.Item1, $"Show {item.Name} Entries ({quests.Count})");
                
                points[item] = (foldout, element.Value.Item2);
                
                if (foldout)
                {
                    foreach (var quest in quests)
                    {
                        EditorGUILayout.BeginHorizontal();

                        foreach (var field in quest.fields)
                        {
                            if (field.title.EndsWith($"{item.Name} Points"))
                            {
                                var label = $"{quest.Name}";
                                if (!field.title.StartsWith($"{item.Name}"))
                                {
                                    label += $" {field.title.Split( $" {item.Name}")[0]}";
                                }

                                label += $" ({field.value} points)";
                                EditorGUILayout.LabelField(label, GUILayout.Width(400));
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space(30);
            
            
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
        private void ResetPoints()
        {
            points.Clear();
            showData = false;
        }
        
        private void FindPoints(DialogueDatabase database)
        {
            ResetPoints();
            
            var allPointTypes = Points.GetAllPointsTypes(database);
            
            for (int i = 0; i < allPointTypes.Count; i++)
            {
                points.Add(allPointTypes[i], (false, Points.MaxScore( allPointTypes[i].Name, database)));
            }
            
            totalPoints = Points.TotalMaxScore(database);
        }

        private void SetMaxPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                points.ElementAt(i).Key.AssignedField("Max Score").value = $"{points.ElementAt(i).Value.Item2}";
            }
        }
    }
}