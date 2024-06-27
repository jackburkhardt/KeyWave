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
        private static SaveGameMetadata _latestSaveData;
        public static SaveGameMetadata LatestSaveData
        {
            set
            {
                _latestSaveData = value;
                OnSaveGameDataReady?.Invoke(value);
            }
            get => _latestSaveData;
        }
        public static bool SaveDataExists => LatestSaveData.state != null;
        public delegate void SaveGameDataDelegate(SaveGameMetadata metadata);
        public static event SaveGameDataDelegate OnSaveGameDataReady;

        /// <summary>
        /// Sends game data to the web interface for persistent storage. Used for saving game state across devices.
        /// </summary>
        /// <param name="savedGameData"></param>
        public static void WebStoreGameData(SavedGameData savedGameData)
        {
            LatestSaveData = new SaveGameMetadata(DateTime.Now, savedGameData);
#if UNITY_WEBGL && !UNITY_EDITOR
            //BrowserInterface.sendSaveGame(JsonConvert.SerializeObject(LatestSaveData));
#endif
        }

        /// <summary>
        /// Stores game data on the local machine at the Application.persistentDataPath. Used for quick-restoring of game data.
        /// </summary>
        /// <param name="savedGameData"></param>
        public static void LocalStoreGameData(SavedGameData savedGameData)
        {
            LatestSaveData = new SaveGameMetadata(DateTime.Now, savedGameData);
            
            Debug.Log("Attempting local save...");
            string saveString = JsonConvert.SerializeObject(LatestSaveData); ;
            PlayerPrefs.SetString("latestLocalSave", saveString);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Fetches the latest save game. Checks local cache first, otherwise returns the latest web save.
        /// </summary>
        /// <returns></returns>
        public static void RetrieveSavedGameData()
        {

            if (PlayerPrefs.HasKey("latestLocalSave"))
            {
                var saveText = PlayerPrefs.GetString("latestLocalSave");
                Debug.Log("Local save size: " + saveText.Length * sizeof(char) / 1024 + "kb");
                //App.Utility.PipeToEditorAndOpen(saveText);
                var localSave = JsonConvert.DeserializeObject<SaveGameMetadata>(saveText);
                if (localSave.last_played > LatestSaveData.last_played)
                {
                    LatestSaveData = localSave;
                }
            }
        }
   }
    
    public struct SaveGameMetadata
    {
        // ReSharper disable InconsistentNaming
        // These names are preferred for JavaScript interop.
        public DateTime last_played;
        public SavedGameData state;
        
        public SaveGameMetadata(DateTime lastPlayed, SavedGameData state)
        {
            this.last_played = lastPlayed;
            this.state = state;
        }
    }
}