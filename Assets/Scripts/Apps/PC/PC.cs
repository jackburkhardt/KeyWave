using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Apps.PC
{
    public class PC : MonoBehaviour
    {
        public static PC Instance;
        [SerializeField] private List<Object> screens = new List<Object>();
        [SerializeField] private Transform appTransform;
        [SerializeField] private GameObject computerGO;
        public bool loggedIn;
        private List<GameObject> screenHistory = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            GameEvent.OnPCUnlock += OnPCUnlock;
            SwitchScreen(!loggedIn ? "LockScreen" : "Search");
        }

        public void OpenPC()
        {
            computerGO.SetActive(true);
            GameEvent.OpenPC();
        }

        public void ClosePC()
        {
            computerGO.SetActive(false);
            GameEvent.ClosePC();
        }
        
        /// <summary>
        /// Goes to the previous screen in the history. If there is none, it closes the computer.
        /// Note: this destroys the current screen from the history.
        /// </summary>
        public void GoBack()
        {
            if (screenHistory.Count <= 1)
            {
                ClosePC();
                return;
            }
            
            Destroy(screenHistory.Last());
            screenHistory.Remove(screenHistory.Last());
        }

        /// <summary>
        /// Goes to the home screen. Note: this will destroy all screens in the history.
        /// </summary>
        public void GoHome()
        {
            while (screenHistory.Count > 1)
            {
                Destroy(screenHistory.Last());
                screenHistory.Remove(screenHistory.Last());
            }
            SwitchScreen(loggedIn ? "Search" : "LockScreen");
        }
        
        // this is only here because unity UI calls need to return void todo: check if need this anymore
        public void UISwitchScreen(string screen) => SwitchScreen(screen);
       
        /// <summary>
        /// Attempts to switch the current PC screen to the one provided. Make sure this screen is in the prefab list in the inspector!
        /// </summary>
        public GameObject SwitchScreen(string screen)
        {
            var screenPrefab = screens.Find(s => s.name == screen);
            if (!screenPrefab)
            {
                Debug.LogError(
                    $"Attempted to switch to screen \"{screen}\" but one by that name was not found. Are you sure the prefab was added to the screen list?");    
                return null;
            }

            //var screenGO = Instantiate(screenPrefab, phoneTransform.position, Quaternion.identity, phoneTransform) as GameObject;
            var screenGO = Instantiate(screenPrefab, appTransform) as GameObject;
            screenHistory.Add(screenGO);
            GameEvent.ChangePCScreen(screen);
            return screenGO;
        }
        
        private void OnPCUnlock()
        {
            loggedIn = true;
            SwitchScreen("Search");
        }

        private void OnDestroy()
        {
            GameEvent.OnPCUnlock -= OnPCUnlock;
        }
    }
}