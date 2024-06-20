using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.App;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public static class SaveDataStorer
    {
        public static SaveGameMetadata LatestSaveData;
        public static bool SaveDataExists => LatestSaveData.state != null;

        /// <summary>
        /// Sends game data to the web interface for persistent storage. Used for saving game state across devices.
        /// </summary>
        /// <param name="savedGameData"></param>
        public static void WebStoreGameData(SavedGameData savedGameData)
        {
            var saveDataWithMeta = new SaveGameMetadata(DateTime.Now, savedGameData);
            
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/DebugSaves")) 
            {
                Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            File.WriteAllTextAsync(Application.dataPath + "/DebugSaves/game.json", JsonConvert.SerializeObject(saveDataWithMeta));
#elif UNITY_WEBGL
            BrowserInterface.sendSaveGame(JsonConvert.SerializeObject(saveDataWithMeta));
#endif
        }

        /// <summary>
        /// Stores game data on the local machine at the Application.persistentDataPath. Used for quick-restoring of game data.
        /// </summary>
        /// <param name="savedGameData"></param>
        public static void LocalStoreGameData(SavedGameData savedGameData)
        {
            var saveDataWithMeta = new SaveGameMetadata(DateTime.Now, savedGameData);
            
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            File.WriteAllTextAsync(Application.dataPath + "/DebugSaves/game.json", JsonConvert.SerializeObject(saveDataWithMeta));
#elif UNITY_WEBGL
            File.WriteAllTextAsync(Application.persistentDataPath + "/game.json", JsonConvert.SerializeObject(saveDataWithMeta));
#endif
        }

        /// <summary>
        /// Fetches the latest save game. Checks local cache first, otherwise returns the latest web save.
        /// </summary>
        /// <returns></returns>
        public static SavedGameData RetrieveSavedGameData()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/DebugSaves"))
            {
                Directory.CreateDirectory(Application.dataPath + "/DebugSaves");
            }
            return JsonConvert.DeserializeObject<SaveGameMetadata>(File.ReadAllText(Application.dataPath + "/DebugSaves/game.json")).state;
#elif UNITY_WEBGL

            if (File.Exists($"{Application.persistentDataPath}/game.json"))
            {
                var localSave = JsonConvert.DeserializeObject<SaveGameMetadata>(File.ReadAllText($"{Application.persistentDataPath}/game.json"));
                if (localSave.last_played > LatestSaveData.last_played)
                {
                    LatestSaveData = localSave;
                    return localSave.state;
                }
            }
            
            if (!SaveDataExists)
            {
                Debug.LogError("No save game data found!");
            }

            return LatestSaveData.state;
#endif
        }
   }
    
    public struct SaveGameMetadata
    {
        public readonly DateTime last_played;
        public readonly SavedGameData state;
        
        public SaveGameMetadata(DateTime lastPlayed, SavedGameData state)
        {
            this.last_played = lastPlayed;
            this.state = state;
        }
    }
}