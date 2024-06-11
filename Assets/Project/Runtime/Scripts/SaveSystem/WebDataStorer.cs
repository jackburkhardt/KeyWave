using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.App;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class WebDataStorer : SavedGameDataStorer
    {
        public static List<int> occupiedSlots = new List<int>();
        public static bool saveDataReady = false;
        public static SavedGameData saveData;

        public override bool HasDataInSlot(int slotNumber)
        {
#if UNITY_EDITOR
            // check if file exists
            return File.Exists(Application.dataPath + "/DebugSaves/" + slotNumber + ".json");
#elif UNITY_WEBGL
            return occupiedSlots.Contains(slotNumber);
#endif
        }

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            File.WriteAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json", JsonUtility.ToJson(savedGameData, true));
#elif UNITY_WEBGL
            BrowserInterface.sendSaveGame(slotNumber, JsonConvert.SerializeObject(savedGameData));
#endif
        }

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            return JsonUtility.FromJson<SavedGameData>(File.ReadAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json"));
#elif UNITY_WEBGL
            saveDataReady = false;
            BrowserInterface.getSaveGame(slotNumber);
            return null;
#endif
        }

        public override void DeleteSavedGameData(int slotNumber)
        {
#if UNITY_EDITOR
            File.Delete(Application.dataPath + "/DebugSaves/" + slotNumber + ".json");
#endif
        }
    }
}