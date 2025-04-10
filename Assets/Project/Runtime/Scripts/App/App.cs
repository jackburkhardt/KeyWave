using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Sentry;
using UnityEngine;
using UnityEngine.SceneManagement;
using Transition = Project.Runtime.Scripts.AssetLoading.LoadingScreen.Transition;

namespace Project.Runtime.Scripts.App
{
    public class App : MonoBehaviour
    {
        private static App _instance;
        private static string playerID;

        public static Action<string> OnSceneLoadStart;
        public static Action<string> OnSceneLoadEnd;
        public static Action<string> OnSceneDeloadStart;
        public static Action<string> OnSceneDeloadEnd;

        public static bool isLoading = false;
        public string currentScene = "StartMenu";

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
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.canYouHearMe();
            BrowserInterface.unityReadyForData();
            SentrySdk.CaptureMessage("Sentry active!");
            #endif
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
            yield return GameManager.instance.StartNewSave();
            GameEvent.OnRegisterPlayerEvent += SendPlayerEvent;
        }

        private IEnumerator BeginGameSequence(bool newGame)
        {
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
            if (e.EventType is "quest_state_change" or "action_state_change")
            {
                if (e.Data["state"].ToString() == "Success")
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                BrowserInterface.sendPlayerEvent(e.ToString());
#endif
                    Debug.Log("[Unity ->] Transmitting player event: " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Loads a scene additively to the current scene
        /// </summary>
        /// 
        public Coroutine LoadScene(string sceneToLoad, string sceneToUnload = "", Transition? transition = Transition.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, transition: transition));

        public Coroutine LoadSceneWithoutLoadingScreen(string sceneToLoad, string sceneToUnload = "") => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, transition: null, waitForUnload: false));
        
        public Coroutine LoadSceneButKeepLoadingScreen(string sceneToLoad, string sceneToUnload = "", Transition? transition = Transition.Default) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, transition: transition, unloadLoadingScreen: false));

        public void UnloadScene(string sceneToUnload)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneToUnload));
        }
        
        public void LoadSceneImmediate(string sceneToLoad, string sceneToUnload = "")
        {
            StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload: sceneToUnload, transition: null));
        }

        /// <summary>
        ///  Unloads the current scene and loads a new scene
        /// </summary>
        public Coroutine ChangeScene(string sceneToLoad, string sceneToUnload, Transition? transition = null) => StartCoroutine(LoadSceneHandler(sceneToLoad, sceneToUnload, transition: transition));

        private IEnumerator LoadSceneHandler(string sceneToLoad, string sceneToUnload = "", Transition? transition = Transition.Default, bool? unloadLoadingScreen = true, bool? waitForUnload = true)
        {
            
            isLoading = true;

            var LoadingScreen = FindObjectOfType<LoadingScreen>();
            
           
            OnSceneDeloadStart?.Invoke(sceneToUnload);
            
            if (transition != null)
            {
                
                if (transition == Transition.None)
                {
                    yield return null;
                }

                else
                {
                    if (LoadingScreen == null)
                    {
                        var loadingSceneLoad = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);

                        while (!loadingSceneLoad.isDone) yield return null;
                
                        LoadingScreen = FindObjectOfType<LoadingScreen>();
                    }

                    if (LoadingScreen != null)
                    {
                        yield return StartCoroutine(LoadingScreen.Show(transition));
                    }

                    else Debug.LogError("Unable to get the canvas group for the loading screen!");
                }
                
               
            }

            var newScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            
            newScene ??= SceneManager.LoadSceneAsync(LocationManager.instance.PlayerLocation.Name, LoadSceneMode.Additive);

            while (!newScene.isDone || !AddressableLoader.IsQueueEmpty()) yield return null;
            
            if (sceneToUnload != "")
            {

                var unloadScene = SceneManager.GetSceneByName(sceneToUnload);
                
                if (unloadScene.IsValid())
                {
                    var unloadCurrentScene = SceneManager.UnloadSceneAsync(unloadScene);
            
                    while (!unloadCurrentScene.isDone)
                    {
                        yield return null;
                    }
                    
                    OnSceneDeloadEnd?.Invoke(sceneToUnload);
                }
        
                
            }
            
            
            OnSceneLoadStart?.Invoke(sceneToLoad);
            
            
            if (waitForUnload == false)
            {
                isLoading = false;
            }
            
            else if (unloadLoadingScreen == true)
            {
                if (LoadingScreen != null)
                {
                    yield return StartCoroutine(LoadingScreen.Hide());
                }

                var loadingScreenScene = SceneManager.GetSceneByName("LoadingScreen");
                if (loadingScreenScene.IsValid())
                {
                    var loadingScreenUnload = SceneManager.UnloadSceneAsync(loadingScreenScene);
        
                    while (!loadingScreenUnload.isDone) yield return null;
                }
              
                isLoading = false;
                OnSceneLoadEnd?.Invoke(sceneToLoad);
                currentScene = sceneToLoad;
            }    
        }
        
    }
}