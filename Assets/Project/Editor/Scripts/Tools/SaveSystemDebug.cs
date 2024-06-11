using System.IO;
using UnityEditor;
using UnityEngine;
using SaveSystem = PixelCrushers.SaveSystem;

namespace Project.Editor.Scripts.Tools
{
    public class SaveSystemDebug 
    {
        [MenuItem("Tools/Perils and Pitfalls/Save System Debug/Save to Slot 1")]
        private static void Save()
        {
            SaveSystem.SaveToSlot(1);
        }

        [MenuItem("Tools/Perils and Pitfalls/Save System Debug/Load from Slot 1")]
        private static void Load()
        {
            SaveSystem.LoadFromSlot(1);
        }

        [MenuItem("Tools/Perils and Pitfalls/Save System Debug/Delete Slot 1")]
        private static void Delete()
        {
            File.Delete(Application.dataPath + "/DebugSaves/" + 1 + ".json");
        }
    }
}