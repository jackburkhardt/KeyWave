

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using SaveSystem = PixelCrushers.SaveSystem;

namespace Project.Editor.Scripts.Tools
{
    public class SaveSlots : EditorWindow
    {
        private static bool _isLoading;
        private const int NUM_SLOTS = 10;
        private static string[] _slotNames;
        private Vector2 _scrollPos;
        
        // [MenuItem("Save System/Load")]
        // private static void ShowLoadWindow()
        // {
        //     _isLoading = true;
        //     ShowWindow();
        // }
        //
        // [MenuItem("Save System/Save")]
        // private static void ShowSaveWindow()
        // {
        //     _isLoading = false;
        //     ShowWindow();
        // }

        private static void ShowWindow()
        {
            if (_slotNames == null)
            {
                try
                {
                    var text = File.ReadAllText($"{Application.dataPath}/DebugSaves/slot_names.json");
                    _slotNames = JsonConvert.DeserializeObject<string[]>(text);
                }
                catch
                {
                    _slotNames = new string[NUM_SLOTS];
                    for (int i = 0; i < NUM_SLOTS; i++)
                    {
                        _slotNames[i] = "";
                    }
                }
            }
            var window = GetWindow<SaveSlots>();
            window.titleContent = new GUIContent((_isLoading ? "[LOAD]" : "[SAVE]") + " Select Slot");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < NUM_SLOTS; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString());
                if (GUILayout.Button("Select"))
                {
                    if (_isLoading)
                    {
                        SaveSystem.LoadFromSlot(i);
                    }
                    else
                    {
                        SaveSystem.SaveToSlot(i);
                    }
                }
                
                bool saveExists = SaveSystem.HasSavedGameInSlot(i);
                GUILayout.Toggle(saveExists, "Has data?");
                
                _slotNames[i] = GUILayout.TextField(_slotNames[i]);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            var saveText = JsonConvert.SerializeObject(_slotNames);
            File.WriteAllText($"{Application.dataPath}/DebugSaves/slot_names.json", saveText);
        }
    }
}