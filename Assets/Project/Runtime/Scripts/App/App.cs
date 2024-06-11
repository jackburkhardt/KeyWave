using System;
using System.Collections;
using System.Collections.Generic;
using KeyWave.Runtime.Scripts.AssetLoading;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.App
{
    public sealed class App : MonoBehaviour
    {
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
        
        private static App _instance;
        private static string playerID;
        private static PlayerEventStack playerEventStack;

        public static Action OnLoadStart;
        public static Action OnLoadEnd;
        
        
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _instance = this;
            if (Camera.main != null) Camera.main.backgroundColor = Color.black;
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.getOccupiedSaveSlots();
            #endif
        }

        public void BeginGame() => StartCoroutine(BeginGameSequence(-1));
        public void BeginGame(int slot) => StartCoroutine(BeginGameSequence(slot));

        private IEnumerator BeginGameSequence(int slot)
        {
            #if !UNITY_EDITOR
            BrowserInterface.canYouHearMe();
            #endif
            yield return LoadSceneButKeepLoadingScreen("Base", sceneToUnload:"StartMenu", type: LoadingScreen.LoadingScreenType.Black);
            while (!DialogueManager.Instance.isInitialized)
            {
                yield return null;
            }
            
            if (PixelCrushers.SaveSystem.HasSavedGameInSlot(slot))
            {
                var data = PixelCrushers.SaveSystem.storer.RetrieveSavedGameData(slot);
                #if !UNITY_EDITOR
                yield return new WaitUntil(() => WebDataStorer.saveDataReady);
                PixelCrushers.SaveSystem.LoadGame(WebDataStorer.saveData);
                #else
                PixelCrushers.SaveSystem.LoadGame(data);
                #endif
            }
            else
            {
                StartCoroutine(GameManager.instance.StartNewSave());
            }
            GameEvent.OnRegisterPlayerEvent += SendPlayerEvent;
        }
        
        private void SendPlayerEvent(PlayerEvent e)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.sendPlayerEvent(e.ToString());
            #endif
        }
        
        /// <summary>
        /// Loads a scene additively to the current scene
        /// </summary>
        /// 
        public Coroutine LoadScene(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? type = LoadingScreen.LoadingScreenType.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: type));
        
        public Coroutine LoadSceneButKeepLoadingScreen(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? type = LoadingScreen.LoadingScreenType.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, loadingScreenType: type, unloadLoadingScreen: false));
        
        public void UnloadScene(string sceneToUnload)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
        }
        
        /// <summary>
        ///  Unloads the current scene and loads a new scene
        /// </summary>
        public Coroutine ChangeScene(string sceneToLoad, string sceneToUnload) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload));

        public static bool isLoading = false;
        
        private IEnumerator LoadSceneHandler(string sceneToLoad, string? sceneToUnload = "", LoadingScreen.LoadingScreenType? loadingScreenType = LoadingScreen.LoadingScreenType.Default, bool? unloadLoadingScreen = true)
        {
            isLoading = true;

            var LoadingScreen = FindObjectOfType<LoadingScreen>();
            
            OnLoadStart?.Invoke();

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

            if (sceneToUnload != "") {
        
                var unloadCurrentScene = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
            
                while (!unloadCurrentScene.isDone)
                {
                    yield return null;
                }
            }

            var newScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

            while (!newScene.isDone || !AddressableLoader.IsQueueEmpty()) yield return null;
            
            if (unloadLoadingScreen == true)
            {
                if (LoadingScreen != null)
                {
                    yield return StartCoroutine(LoadingScreen.FadeCanvasOut());
                }

                var loadingScreenUnload = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("LoadingScreen"));
        
                while (!loadingScreenUnload.isDone) yield return null;
                Debug.Log("Load complete");
                
                isLoading = false;
                OnLoadEnd?.Invoke();
               
            }
        }
    }
}