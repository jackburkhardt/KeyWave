using System;
using UnityEngine;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.SaveSystem;
using TMPro;

namespace Project.Runtime.Scripts.UI
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _continueButton;
        [SerializeField] private TextMeshProUGUI _continueTimestamp;
        
        [SerializeField] private GameObject _saveExistsWarningPopup;
        [SerializeField] private TextMeshProUGUI _saveExistsWarningTimestamp;

        private void Awake()
        {
           SaveDataStorer.OnSaveGameDataReady += OnSaveDataReceived;
        }

        private void OnSaveDataReceived(SaveGameMetadata metadata)
        {
            _continueButton.SetActive(true);
            _continueTimestamp.text = metadata.last_played.ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        }
        
        public void BeginSaveRetrieval()
        {
            SaveDataStorer.RetrieveSavedGameData();
        }
        
        public void TryStartNewGame()
        {
            if (!SaveDataStorer.SaveDataExists)
            {
                StartNewGame();
                return;
            }
            
            _saveExistsWarningPopup.SetActive(true);
            string saveTimestamp = SaveDataStorer.LatestSaveData.last_played.ToLocalTime().ToString("MM/dd/yyyy HH:mm");
            _saveExistsWarningTimestamp.text =
                $"Existing progress found! Last played: {saveTimestamp}.\nStart a new game and overwrite this data?";
        }
        
        public void StartNewGame()
        {
            App.App.Instance.StartNewGame();
        }

        public void ContinueGame()
        {
           App.App.Instance.ContinueGame();
        }
        
        private void OnDestroy()
        {
            SaveDataStorer.OnSaveGameDataReady -= OnSaveDataReceived;
        }
    }
}