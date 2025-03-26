using System;
using DG.Tweening;
using PixelCrushers;
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
        
        [SerializeField] private UIPanel _saveExistsWarningPopup;
        [SerializeField] private UIPanel _mainMenuButtonsPanel;
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
            
            _saveExistsWarningPopup.Open();
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
            GameManager.instance.inputManager.pauseButton.TogglePause();
        }
        
        public void BeginSaveRetrieval()
        {
            SaveDataStorer.BeginSaveRetrieval();
        }

        public void OnFirstClick()
        {
            GetComponent<Animator>().SetTrigger("Click");
            SaveDataStorer.BeginSaveRetrieval();
            DOTween.Sequence( )
                .AppendInterval(2f)
                .AppendCallback(() => _mainMenuButtonsPanel.Open());
        }
        
        private void OnDestroy()
        {
            SaveDataStorer.OnSaveGameDataReady -= OnSaveDataReceived;
        }
    }
}