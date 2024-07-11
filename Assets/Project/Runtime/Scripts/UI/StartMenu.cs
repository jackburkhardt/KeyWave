using System;
using System.Linq;
using UnityEngine;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using TMPro;
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
           _continueButton.SetActive(false);
           RefreshLayoutGroups.Refresh(_continueButton.transform.parent.gameObject);
           SaveDataStorer.OnSaveGameDataReady += OnSaveDataReceived;
           
           #if UNITY_EDITOR
           var lamb = new GameObject("Sacrificial Lamb");
           DontDestroyOnLoad(lamb);

           var sheepList = new[] { "Evan", "Save System", "App", "[Debug Updater]"};
           
           foreach(var suspect in lamb.scene.GetRootGameObjects())
               if (!sheepList.Contains(suspect.name)) Destroy(suspect);
           #endif
        }
        

        private void OnSaveDataReceived(SaveGameMetadata metadata)
        {
            _continueButton.SetActive(true);
            RefreshLayoutGroups.Refresh(_continueButton.transform.parent.gameObject);
            _continueTimestamp.text = metadata.last_played.ToLocalTime().ToString("MM/dd/yyyy HH:mm");
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
            _onNewGame.Invoke();
            App.App.Instance.StartNewGame();
        }

        public void ContinueGame()
        {
            _onContinue.Invoke();
           App.App.Instance.ContinueGame();
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