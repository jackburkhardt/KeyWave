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
        public static extern void sendSaveGame(int slot, string saveData);

        [DllImport("__Internal")]
        public static extern void getSaveGame(int slot);

        [DllImport("__Internal")]
        public static extern void getOccupiedSaveSlots();

        // JSON data received from web interface. Updates the PixelCrushers save storer.
        public void getOccupiedSaveSlotsCallback(string jsonData)
        {
            Debug.Log("Slot data: " + jsonData);
            List<int> slots = JsonConvert.DeserializeObject<List<int>>(jsonData); 
            if (slots == null) return;
            WebDataStorer.occupiedSlots = slots;
        }

        public void getSaveGameCallback(string jsonData)
        {
            SavedGameData data = JsonConvert.DeserializeObject<SavedGameData>(jsonData);
            if (data != null)
            {
                WebDataStorer.saveData = data;
                WebDataStorer.saveDataReady = true;
            }
        }
    }
}