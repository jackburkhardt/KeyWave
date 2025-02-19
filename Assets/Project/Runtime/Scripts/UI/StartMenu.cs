using System;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.UI
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _continueButton;
        [SerializeField] private TextMeshProUGUI _continueTimestamp;
        
        [SerializeField] private GameObject _saveExistsWarningPopup;
        [SerializeField] private TextMeshProUGUI _saveExistsWarningTimestamp;
        [SerializeField] private UnityEvent _onNewGame;
        [SerializeField] private UnityEvent _onContinue;

        private void Awake()
        {
            if (_continueButton != null)
            {
                if (!SaveDataStorer.SaveDataExists)
                    _continueButton.SetActive(false);
                else
                    _continueButton.SetActive(true);
                
                
                RefreshLayoutGroups.Refresh(_continueButton.transform.parent.gameObject);
            }

            SaveDataStorer.OnSaveGameDataReady += OnSaveDataReceived;
        }

        private void OnEnable()
        {
            UserSettingsSaver.ApplySettings();
        }

        private void OnSaveDataReceived(SaveGameMetadata metadata)
        {
            if (_continueButton == null) return;
            _continueButton.SetActive(true);
            RefreshLayoutGroups.Refresh(_continueButton.transform.parent.gameObject);
            _continueTimestamp.text = metadata.last_played.ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        }
        
        public void TryStartNewGame()
        {
            if (!SaveDataStorer.SaveDataExists || _continueButton == null)
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
            _onNewGame.Invoke();
            App.App.Instance.StartNewGame();
        }

        public void ContinueGame()
        {
            _onContinue.Invoke();
           App.App.Instance.ContinueGame();
        }
        
        public void PauseGame()
        {
            GameManager.instance.TogglePause();
        }
        
        public void BeginSaveRetrieval()
        {
            SaveDataStorer.BeginSaveRetrieval();
        }
        
        private void OnDestroy()
        {
            SaveDataStorer.OnSaveGameDataReady -= OnSaveDataReceived;
        }
    }
}