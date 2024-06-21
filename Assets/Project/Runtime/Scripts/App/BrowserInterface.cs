using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.SaveSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.App
{
    public class BrowserInterface : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void canYouHearMe();

        [DllImport("__Internal")]
        public static extern void sendPlayerEvent(string playerEvent);

        [DllImport("__Internal")]
        public static extern void sendSaveGame(string saveData);
        
        public delegate void SaveGameDataDelegate(SaveGameMetadata metadata);
        public static event SaveGameDataDelegate OnSaveGameDataReceived;
        
        public static void gameStateReceived(string jsonData)
        {
            Debug.Log("Got game state from web interface: " + jsonData);
            SaveGameMetadata saveGameMetadata = JsonConvert.DeserializeObject<SaveGameMetadata>(jsonData);
            SaveDataStorer.LatestSaveData = saveGameMetadata;
            OnSaveGameDataReceived?.Invoke(saveGameMetadata);
        }
    }
}