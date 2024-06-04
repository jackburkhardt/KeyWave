using PixelCrushers;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class WebDataStorer : SavedGameDataStorer
    {
        public override bool HasDataInSlot(int slotNumber)
        {
            #if UNITY_EDITOR
            // check if file exists
            return System.IO.File.Exists(Application.dataPath + "/DebugSaves/" + slotNumber + ".json");
            #endif
        }

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
            #if UNITY_EDITOR
             System.IO.File.WriteAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json", JsonUtility.ToJson(savedGameData, true));
            #endif
        }

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
            #if UNITY_EDITOR
            return JsonUtility.FromJson<SavedGameData>(System.IO.File.ReadAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json"));
            #endif
        }

        public override void DeleteSavedGameData(int slotNumber)
        {
            #if UNITY_EDITOR
            System.IO.File.Delete(Application.dataPath + "/DebugSaves/" + slotNumber + ".json");
            #endif
        }
    }
}