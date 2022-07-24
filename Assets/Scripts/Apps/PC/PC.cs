using System;
using System.Collections.Generic;
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
        }

        public void OpenPC()
        {
            computerGO.SetActive(true);
            SwitchScreen(!loggedIn ? "LockScreen" : "Search");
            GameEvent.OpenPC();
        }

        public void ClosePC()
        {
            computerGO.SetActive(false);
            GameEvent.ClosePC();
        }
        
        
        public void UISwitchScreen(string screen) => SwitchScreen(screen);
        public GameObject SwitchScreen(string screen)
        {
            var screenPrefab = screens.Find(s => s.name == screen);
            if (!screenPrefab)
            {
                Debug.LogError("Attempted to switch to screen \"" + screen + "\" but one by that name was not found. Are you sure the prefab was added to the screen list?");    
                return null;
            }

            //var screenGO = Instantiate(screenPrefab, phoneTransform.position, Quaternion.identity, phoneTransform) as GameObject;
            var screenGO = Instantiate(screenPrefab, appTransform) as GameObject;
            screenHistory.Add(screenGO);
            return screenGO;
        }
    }
}