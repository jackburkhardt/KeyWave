using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class SaveDataStorer : SavedGameDataStorer
    {
        private static SaveGameMetadata _latestSaveData;
        public static SaveGameMetadata LatestSaveData
        {
            private set
            {
                _latestSaveData = value;
                OnSaveGameDataReady?.Invoke(value);
            }
            get => _latestSaveData;
        }
        public static bool SaveDataExists => LatestSaveData.state != null;
        public delegate void SaveGameDataDelegate(SaveGameMetadata metadata);
        public static event SaveGameDataDelegate OnSaveGameDataReady;
        private static bool savingEnabled => DialogueLua.GetVariable("saving_enabled").asBool;

        /// <summary>
        /// Sends game data to the web interface for persistent storage. Used for saving game state across devices.
        /// </summary>
        /// <param name="savedGameData"></param>
        public static void WebStoreGameData(SavedGameData savedGameData)
        {
            if (!savingEnabled) return;
            
            LatestSaveData = new SaveGameMetadata(DateTime.Now, savedGameData);
#if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.sendSaveGame(PixelCrushers.SaveSystem.Serialize(LatestSaveData));
#endif
        }

        /// <summary>
        /// Fetches the latest save game. Checks local cache first, otherwise returns the latest web save.
        /// </summary>
        /// <returns></returns>
        private static void CheckDiskForSaveData()
        {
            if (File.Exists($"{Application.persistentDataPath}/save.json"))
            {
                var saveText = File.ReadAllText($"{Application.persistentDataPath}/save.json");
                Debug.Log("[<- Unity] Local save size: " + saveText.Length * sizeof(char) / 1024 + "kb");

                var localSave = PixelCrushers.SaveSystem.Deserialize<SaveGameMetadata>(saveText);
                if (localSave.last_played > LatestSaveData.last_played)
                {
                    LatestSaveData = localSave;
                }
            }
        }

        public void WebSaveGameCallback(string json)
        {
            Debug.Log("[<- Unity] Received data from web server: " + json);
            if (string.IsNullOrEmpty(json)) return;
            
            var saveData = PixelCrushers.SaveSystem.Deserialize<SaveGameMetadata>(json);
            if (saveData.last_played > LatestSaveData.last_played)
            {
                LatestSaveData = saveData;
            }
        }
        
        public static void BeginSaveRetrieval()
        {
            CheckDiskForSaveData();
        }

        public override bool HasDataInSlot(int slotNumber)
        {
            return true;
            //return PixelCrushers.SaveSystem.HasSavedGameInSlot(slotNumber);
        }
        
        private string m_playerPrefsKeyBase = "Save";
        
        public string GetPlayerPrefsKey(int slotNumber)
        {
            return m_playerPrefsKeyBase + slotNumber;
        }
        

        public override void StoreSavedGameData(int slotNumber, SavedGameData savedGameData)
        {
            if (!savingEnabled) return;

            if (slotNumber == 1)
            {
                LatestSaveData = new SaveGameMetadata(DateTime.Now, savedGameData);

                string saveString = PixelCrushers.SaveSystem.Serialize(LatestSaveData);

                //  Debug.Log("Performing local autosave...");
                File.WriteAllText($"{Application.persistentDataPath}/save.json", saveString);
#if UNITY_WEBGL && !UNITY_EDITOR // todo: see if this can be removed for optimization
            Application.ExternalEval("_JS_FileSystem_Sync();");
#endif
            }

            else
            {
                var s = PixelCrushers.SaveSystem.Serialize(savedGameData);
                
                PlayerPrefs.SetString(GetPlayerPrefsKey(slotNumber),s);
                PlayerPrefs.Save();
            }
        }
        
        

        public override SavedGameData RetrieveSavedGameData(int slotNumber)
        {
            
            if (slotNumber == 1) return LatestSaveData.state;
            
            
            var s = PlayerPrefs.GetString(GetPlayerPrefsKey(slotNumber));
            return HasDataInSlot(slotNumber) ? PixelCrushers.SaveSystem.Deserialize<SavedGameData>(s) : new SavedGameData();
          
          //  var s = PlayerPrefs.GetString(GetPlayerPrefsKey(slotNumber));
           // return HasDataInSlot(slotNumber) ? SaveSystem.Deserialize<SavedGameData>(s) : new SavedGameData();
            
          //  return LatestSaveData.state;
        }

        public override void DeleteSavedGameData(int slotNumber)
        {
            File.Delete($"{Application.dataPath}/DebugSaves/{slotNumber}.json");
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