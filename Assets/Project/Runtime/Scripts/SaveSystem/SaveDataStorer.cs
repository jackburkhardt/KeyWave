using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
#if UNITY_WEBGL && !UNITY_EDITOR
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
            
            PlayerPrefs.SetString("latestLocalSave", JsonConvert.SerializeObject(saveDataWithMeta));
        }

        /// <summary>
        /// Fetches the latest save game. Checks local cache first, otherwise returns the latest web save.
        /// </summary>
        /// <returns></returns>
        public static SavedGameData RetrieveSavedGameData()
        {

            if (PlayerPrefs.HasKey("latestLocalSave"))
            {
                var saveText = PlayerPrefs.GetString("latestLocalSave");
                Debug.Log("Local save size: " + saveText.Length * sizeof(char) / 1024 + "kb");
                var localSave = JsonConvert.DeserializeObject<SaveGameMetadata>(saveText);
                if (localSave.last_played > LatestSaveData.last_played)
                {
                    LatestSaveData = localSave;
                    return localSave.state;
                }
            }
            
            if (!SaveDataExists)
            {
                Debug.LogWarning("No save game data found!");
            }

            return LatestSaveData.state;
        }
   }
    
    public struct SaveGameMetadata
    {
        public DateTime last_played;
        public SavedGameData state;
        
        public SaveGameMetadata(DateTime lastPlayed, SavedGameData state)
        {
            this.last_played = lastPlayed;
            this.state = state;
        }
    }
}