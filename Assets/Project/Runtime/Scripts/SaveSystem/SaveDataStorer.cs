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
    public class SaveDataStorer : SavedGameDataStorer
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
            string saveString = JsonConvert.SerializeObject(LatestSaveData);
           // Debug.Log("Save string : " + saveString);
            File.WriteAllText($"{Application.persistentDataPath}/save.json", saveString);
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("_JS_FileSystem_Sync();");
#endif
        }

        /// <summary>
        /// Fetches the latest save game. Checks local cache first, otherwise returns the latest web save.
        /// </summary>
        /// <returns></returns>
        public static void RetrieveSavedGameData()
        {
            if (File.Exists($"{Application.persistentDataPath}/save.json"))
            {
                var saveText = File.ReadAllText($"{Application.persistentDataPath}/save.json");
                Debug.Log("Local save size: " + saveText.Length * sizeof(char) / 1024 + "kb");
                //Debug.Log("Local save: " + saveText);
                var localSave = JsonConvert.DeserializeObject<SaveGameMetadata>(saveText);
                if (localSave.last_played > LatestSaveData.last_played)
                {
                    LatestSaveData = localSave;
                }
            }
        }

        public override bool HasDataInSlot(int slotNumber)
        {
            return slotNumber == 1;
        }

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
            LocalStoreGameData(savedGameData);
        }

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
            return LatestSaveData.state;
        }

        public override void DeleteSavedGameData(int slotNumber)
        {
            throw new NotImplementedException();
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