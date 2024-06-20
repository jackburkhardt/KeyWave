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
            BrowserInterface.OnSaveGameDataReceived += OnSaveDataReceived;
        }

        private void OnSaveDataReceived(SaveGameMetadata metadata)
        {
            _continueButton.SetActive(true);
        }
        
        public void TryStartNewGame()
        {
            if (SaveDataStorer.SaveDataExists)
            {
                _saveExistsWarningPopup.SetActive(true);
                _saveExistsWarningTimestamp.text = SaveDataStorer.LatestSaveData.last_played.ToString("MM/dd/yyyy HH:mm");
            }
            else
            {
                StartNewGame();
            }
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
            BrowserInterface.OnSaveGameDataReceived -= OnSaveDataReceived;
        }
    }
}