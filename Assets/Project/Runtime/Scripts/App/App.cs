using System;
using System.Collections;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.App
{
    public class App : MonoBehaviour
    {
        private static App _instance;
        private static string playerID;
        private static PlayerEventStack playerEventStack;

        public static Action OnLoadStart;
        public static Action OnLoadEnd;

        public static bool isLoading = false;

        public static App Instance
        {
            get
            {
                if (_instance) return _instance;
                
                var go = new GameObject("App (Lazy Init)");
                _instance = go.AddComponent<App>();
                Debug.LogWarning("App instance not found, a new one has been initialized. This is OK for the Editor, but should not happen otherwise.");
                return _instance;
            }
        }

        public static string PlayerID => playerID;


        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(this.gameObject);
            }
            
            if (Camera.main != null) Camera.main.backgroundColor = Color.black;
            Cursor.lockState = CursorLockMode.None;
        }
        
        

        private void Start()
        {
            BrowserInterface.canYouHearMe();
            BrowserInterface.unityReadyForData();
        }

        public void StartNewGame()
        {
            StartCoroutine( NewGameSequence());
        }

        public void ContinueGame()
        {
            StartCoroutine(BeginGameSequence(false));
        }
        
        

        private IEnumerator NewGameSequence()
        {
            yield return LoadSceneWithoutLoadingScreen("Base");
            

            yield return GameManager.instance.StartNewSave();
            GameEvent.OnRegisterPlayerEvent += SendPlayerEvent;
        }

        private IEnumerator BeginGameSequence(bool newGame)
        {
            yield return LoadSceneButKeepLoadingScreen("Base", sceneToUnload:"StartMenu", type: LoadingScreen.LoadingScreenType.Black);
            while (!DialogueManager.Instance.isInitialized)
            {
                yield return null;
            }
            
            if (!newGame)
            {
                PixelCrushers.SaveSystem.LoadFromSlot(1);
            }
            else
            {
                StartCoroutine(GameManager.instance.StartNewSave());
            }
            GameEvent.OnRegisterPlayerEvent += SendPlayerEvent;
        }

        private void SendPlayerEvent(PlayerEvent e)
        {
            if (e.EventType == "quest_state_change")
            {
    #if UNITY_WEBGL && !UNITY_EDITOR
                BrowserInterface.sendPlayerEvent(e.ToString());
    #endif
            }
        }

        /// <summary>
        /// Loads a scene additively to the current scene
        /// </summary>
        /// 
        public Coroutine LoadScene(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? type = LoadingScreen.LoadingScreenType.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: type));

        public Coroutine LoadSceneWithoutLoadingScreen(string sceneToLoad, string? sceneToUnload = "") => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: null, waitForUnload: false));
        
        public Coroutine LoadSceneButKeepLoadingScreen(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? type = LoadingScreen.LoadingScreenType.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: type, unloadLoadingScreen: false));

        public void UnloadScene(string sceneToUnload)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
        }
        
        public void LoadSceneImmediate(string sceneToLoad, string sceneToUnload = "")
        {
            StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: null));
        }

        /// <summary>
        ///  Unloads the current scene and loads a new scene
        /// </summary>
        public Coroutine ChangeScene(string sceneToLoad, string sceneToUnload, LoadingScreen.LoadingScreenType? type = null) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload, loadingScreenType: type));

        private IEnumerator LoadSceneHandler(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? loadingScreenType = LoadingScreen.LoadingScreenType.Default, bool? unloadLoadingScreen = true, bool? waitForUnload = true)
        {
            
            isLoading = true;

            var LoadingScreen = FindObjectOfType<LoadingScreen>();
            
            OnLoadStart?.Invoke();
            
            if (loadingScreenType != null)
            {
                if (LoadingScreen == null)
                {
                    var loadingSceneLoad = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);

                    while (!loadingSceneLoad.isDone) yield return null;
                
                    LoadingScreen = FindObjectOfType<LoadingScreen>();
                }

                if (LoadingScreen != null)
                {
                    yield return StartCoroutine(LoadingScreen.FadeCanvasIn(loadingScreenType));
                }

                else Debug.LogError("Unable to get the canvas group for the loading screen!");
            }

            if (sceneToUnload != "") {
        
                var unloadCurrentScene = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
            
                while (!unloadCurrentScene.isDone)
                {
                    yield return null;
                }
            }
         

            var newScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

            while (!newScene.isDone || !AddressableLoader.IsQueueEmpty()) yield return null;
            
            
            if (waitForUnload == false)
            {
                isLoading = false;
            }
            
            else if (unloadLoadingScreen == true)
            {
                if (LoadingScreen != null)
                {
                    yield return StartCoroutine(LoadingScreen.FadeCanvasOut());
                }

                var loadingScreenUnload = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("LoadingScreen"));
        
                while (!loadingScreenUnload.isDone) yield return null;
              
                isLoading = false;
                OnLoadEnd?.Invoke();
            }

            
        }
        
    }
}