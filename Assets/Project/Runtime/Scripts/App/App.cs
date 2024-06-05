using System;
using System.Collections;
using System.Collections.Generic;
using KeyWave.Runtime.Scripts.AssetLoading;
using PixelCrushers.DialogueSystem;
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
        
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _instance = this;
            Camera.current.backgroundColor = Color.black;
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.getSocketLibrarySource();
            #endif
        }

        public void BeginGame() => StartCoroutine(BeginGameSequence());

        private IEnumerator BeginGameSequence()
        {
            #if !UNITY_EDITOR
            BrowserInterface.canYouHearMe();
            #endif
            yield return SceneManager.LoadSceneAsync("Base");
            while (!DialogueManager.Instance.isInitialized)
            {
                yield return null;
            }
            
            if (PixelCrushers.SaveSystem.HasSavedGameInSlot(1))
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
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.sendPlayerEvent(e.ToString());
            #endif
        }
        
        /// <summary>
        /// Loads a scene additively to the current scene
        /// </summary>
        public Coroutine LoadScene(string sceneToLoad) => StartCoroutine(LoadSceneHandler(sceneToLoad));
        
        public void UnloadScene(string sceneToUnload)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
        }
        
        /// <summary>
        ///  Unloads the current scene and loads a new scene
        /// </summary>
        public Coroutine ChangeScene(string sceneToLoad, string sceneToUnload) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload));

        private bool isLoading = false;
        private IEnumerator LoadSceneHandler(string sceneToLoad, string sceneToUnload = "")
        {
            if (isLoading)
            {
                Debug.LogError($"Attempted to load scene {sceneToLoad} while another scene was loading! Aborting.");
                yield break;
            }
            isLoading = true;
        
            var loadingScene = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);

            while (!loadingScene.isDone) yield return null;

            var LoadingScreen = FindObjectOfType<LoadingScreen>();

            if (LoadingScreen != null)
            {
                yield return StartCoroutine(LoadingScreen.FadeCanvasIn());
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
            
            if (LoadingScreen != null)
            {
                yield return StartCoroutine(LoadingScreen.FadeCanvasOut());
            }

            var unloadLoadingScreen = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Loading"));
        
            while (!unloadLoadingScreen.isDone) yield return null;
            Debug.Log("Load complete");
            
            isLoading = false;

        }
    }
}