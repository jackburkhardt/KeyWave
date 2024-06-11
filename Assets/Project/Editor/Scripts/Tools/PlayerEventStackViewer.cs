using System;
using System.IO;
using Project.Runtime.Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class PlayerEventStackViewer : EditorWindow
    {
        private Vector2 scrollPos;

        private void OnGUI()
        {
            if (GameManager.instance == null)
            {
                EditorGUILayout.LabelField("The game is not running.");
                return;
            }
            
            if (GameManager.playerEventStack == null)
            {
                EditorGUILayout.LabelField("The player event stack is not initialized.");
                return;
            }
            
            var eventStack = GameManager.playerEventStack;

            if (GUILayout.Button("Save to Disk (KeyWave/Logs)"))
            {
                var pathWithoutAssets = Application.dataPath[..^6];
                var fileName = "PlayerEvents_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
                File.WriteAllText($"{pathWithoutAssets}/Logs/{fileName}", eventStack.SerializeEvents());
                Console.WriteLine($"PlayerEventStack saved to {pathWithoutAssets}/Logs/{fileName}");
            }
            EditorGUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var playerEvent in eventStack.RegisteredEvents)
            {
                GUILayout.TextArea(playerEvent.ToString(), GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Perils and Pitfalls/Viewer/Player Event Stack Viewer")]
        private static void ShowWindow()
        {
            var window = GetWindow<PlayerEventStackViewer>();
            window.titleContent = new GUIContent("Player Event Stack");
            window.Show();
        }
    }
}