using PixelCrushers.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameStateManager gameStateManager;
    private PlayerEventStack playerEventStack;
    private CustomLuaFunctions _customLuaFunctions;
    public List<Location> locations;

    public static GameState gameState;

    // Start is called before the first frame update
    private void Awake()
    {
        //if instance of GameManager already exists, destroy it

        if (instance == null)
        {
            instance = this;
        }
        
        else if (instance != this)
        {
            Destroy(this);
        }
        
        
        if (gameStateManager == null)
        {
            gameStateManager = this.AddComponent<GameStateManager>();
        }
        
        gameState = gameStateManager.gameState;
        
        if (playerEventStack == null)
        {
            playerEventStack = this.AddComponent<PlayerEventStack>();
        }
        _customLuaFunctions = GetComponent<CustomLuaFunctions>() ?? gameObject.AddComponent<CustomLuaFunctions>();

    }
  
    public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)

    {
        var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layoutGroup in componentsInChildren)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        var parent = root.GetComponent<LayoutGroup>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
    }
  
    private void Start()
    {
        StartCoroutine(StartHandler());
    }

    private void OnEnable()
    {
       GameStateManager.OnGameStateChanged += OnGameStateChange;
    }
    
    private void OnDisable()
    {
        GameStateManager.OnGameStateChanged -= OnGameStateChange;
    }
    
    private void OnGameStateChange(GameState gameState)
    {
        switch (gameState.type)
        {
            case GameState.Type.Normal:
                if (gameState.clock > Clock.DailyLimit)
                {
                    gameStateManager.SetGameStateType(GameState.Type.EndOfDay);
                    DialogueManager.StopConversation();
                    DialogueManager.StartConversation("EndOfDay");
                }
                break;
            case GameState.Type.EndOfDay:
                break;
        }
    }

    public void StartNewDay()
    {
        GameStateManager.instance.StartNextDay();
        LoadScene("StartOfDay");
    }


    IEnumerator StartHandler()
    {
        yield return StartCoroutine(playerEventStack.RunEvents());

        yield return StartCoroutine(gameStateManager.LoadGameState());
    }

    public static string GetHighestDialogueNodeValue()
    {
        string value = string.Empty;
        
        var dialogueText = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.currentDialogueText;
        var dialogueEntryTitle = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.Title;
        var menuText = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.currentMenuText;
        string[] textToSerialize = { dialogueText, menuText, dialogueEntryTitle };

        for (int i = 0; i < textToSerialize.Length; i++)
        {
            if (textToSerialize[i] == string.Empty) continue;
            value = textToSerialize[i];
        }

        return value;
    }

   

    public void LoadScene(string sceneName) => StartCoroutine(LoadSceneHandler(sceneName));
    
    public void ChangeScene(string currentScene, string newScene) => StartCoroutine(LoadSceneHandler(newScene, currentScene));

    public void OpenMap() => ChangeScene(gameStateManager.gameState.player_location, "Map");

    public void EndOfDay() => ChangeScene(gameStateManager.gameState.player_location, "EndOfDay");
    
    public void TravelTo(string newLocation, string currentScene = "Map")
    {
        StartCoroutine(TravelToHandler());

        IEnumerator TravelToHandler()
        {
            yield return StartCoroutine(LoadSceneHandler(newLocation, currentScene));
            
            yield return new WaitForSeconds(0.5f);
            
            DialogueManager.StartConversation($"{newLocation}/Base");
        }
    }
    
    public void Wait(int duration)
    {
       GameEvent.OnWait(duration);
    }

    public IEnumerator LoadSceneHandler(string newScene, string currentScene = "")
    {
        var currentSceneName = $"Resources/Scenes/{currentScene}";
        var newSceneName = $"Resources/Scenes/{newScene}";
        var loadingSceneName = $"Resources/Scenes/Loading";
        var loadingScene = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        
        while (!loadingScene.isDone)
        {
            yield return null;
        }

        var LoadingScreen = FindObjectOfType<LoadingScreen>();
        
        if (LoadingScreen != null)
        {
            yield return StartCoroutine(LoadingScreen.FadeCanvasIn());
        }
        
        if (currentScene != "") {
        
            var unloadCurrentScene = SceneManager.UnloadSceneAsync(currentSceneName);
            
            while (!unloadCurrentScene.isDone)
            {
                yield return null;
            }
        }
        
        var loadNewScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        
        while (!loadNewScene.isDone)
        {
            yield return null;
        }
        
        if (LoadingScreen != null)
        {
            yield return StartCoroutine(LoadingScreen.FadeCanvasOut());
        }

        var unloadLoadingScreen = SceneManager.UnloadSceneAsync(loadingSceneName);
        
           while (!unloadLoadingScreen.isDone)
           {
               yield return null;
           }
    }
    
   
    
}
