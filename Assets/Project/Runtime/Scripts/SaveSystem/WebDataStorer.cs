using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.App;
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
#elif UNITY_WEBGL
            return BrowserInterface.saveGameExists(slotNumber);
#endif
        }

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
#if UNITY_EDITOR
            if (!System.IO.Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            System.IO.File.WriteAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json", JsonUtility.ToJson(savedGameData, true));
#elif UNITY_WEBGL
            BrowserInterface.sendSaveGame(slotNumber, JsonConvert.SerializeObject(savedGameData));
#endif
        }

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
#if UNITY_EDITOR
            if (!System.IO.Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            return JsonUtility.FromJson<SavedGameData>(System.IO.File.ReadAllText(Application.dataPath + "/DebugSaves/" + slotNumber + ".json"));
#elif UNITY_WEBGL
            return JsonConvert.DeserializeObject<SavedGameData>(BrowserInterface.getSaveGame(slotNumber));
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