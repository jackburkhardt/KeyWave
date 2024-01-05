using PixelCrushers.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class GameData
{
    public string player_location;
}


public class GameManager : MonoBehaviour
{
    // gamemanger instance
    
    public static GameManager instance;
    public static DialogueSystemController dialogueSystemController;

    [SerializeField] private GameObject dialogueSystem;

    //chapter
    public static string currentModule;
    public static string currentChapter;
    //actor
    public static string playerLocation;

    [SerializeField] private SceneAsset UI;


    public enum Modules
    {
        PerilsAndPitfalls,
        Yellowtail
    };

    public enum Chapters
    {
        Prologue,
        Day1,
        Day2,
        Day3,
        Epilogue,
        Debug
    }

    [SerializeField]
    Modules module;
    [SerializeField]
    Chapters chapter;


    private string _path;
    public GameData gameData;

    public Scene currentScene;

    // Start is called before the first frame update
    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene();
        
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        if (dialogueSystemController == null)
        {
            Instantiate(dialogueSystem);
            dialogueSystemController = dialogueSystem.GetComponent<DialogueSystemController>();
        }
        else if (dialogueSystemController != dialogueSystem.GetComponent<DialogueSystemController>())
        {
            Destroy(dialogueSystem);
        }

        dialogueSystem.gameObject.SetActive(true);
        
        DontDestroyOnLoad(gameObject);
        currentModule = module.ToString();
        currentChapter = chapter.ToString();

        StartCoroutine(SetPath());
        IEnumerator SetPath()
        {
            while (GameManager.currentModule == null) yield return null;
            _path = $"{Application.dataPath}/Resources/GameData/{GameManager.currentModule}/Game.json";
        }
    }

    private void OnEnable()
    {
        GameEvent.OnPlayerEvent += ExecutePlayerEvent;
    }

    bool gameIsLoading;

    private void LoadGame()
    {
        
      //  ExecutePlayerEvent(new PlayerEvents.PlayerEvent("move", "GameManager", "player", "Lobby"));
        SceneManager.LoadSceneAsync(UI.name, LoadSceneMode.Additive);
        StartCoroutine("LoadGameFromStart");
       //DialogueManager.conversationView.
    }
    
    IEnumerator StartConversationAfterOneFrame(Transform actor, Transform conversant)
    {
        yield return new WaitForEndOfFrame();
        MostRecentDialogueTrigger = "GameManager";
        DialogueManager.StartConversation(CurrentConversationValues.Title, actor, conversant, CurrentConversationValues.LineID);
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator LoadGameFromStart()
    {
        while (PlayerEventStack.eventsWrapper == null) yield return null;
            
        Debug.Log("loading " + PlayerEventStack.eventsWrapper.events.Count() + " events");
        this.gameIsLoading = true;
            
        for (int i = 0; i < PlayerEventStack.eventsWrapper.events.Count(); i++)
        {
           // Debug.Log("executing " + PlayerEventStack.eventsWrapper.events[i].Type + " event from " + PlayerEventStack.eventsWrapper.events[i].Sender + " to " + PlayerEventStack.eventsWrapper.events[i].Receiver + " with value " + PlayerEventStack.eventsWrapper.events[i].Value);
            ExecutePlayerEvent(PlayerEventStack.eventsWrapper.events[i]);
        }

        this.gameIsLoading = false;
        
        //load main UI
        yield return StartCoroutine(LoadSceneAsynchronously(currentScene, playerLocation));
        // Debug.Log("starting conversation " + CurrentConversationValues.Title);s
       
        DialogueManager.PreloadDialogueUI();
        DialogueManager.PreloadMasterDatabase();

        if (CurrentConversationValues.Title == string.Empty) yield break;
        var actor = string.IsNullOrEmpty(CurrentConversationValues.Actor)
            ? null
            : GameObject.Find(CurrentConversationValues.Actor);
        var conversant = string.IsNullOrEmpty(CurrentConversationValues.Conversant)
            ? null
            : GameObject.Find(CurrentConversationValues.Conversant);
        var actorTransform = (actor != null) ? actor.transform : null;
        var conversantTransform = (conversant != null) ? conversant.transform : null;

        yield return StartCoroutine(StartConversationAfterOneFrame(actorTransform, conversantTransform));
    }
    
    IEnumerator LoadSceneAsynchronously(string oldScene, string newScene)
    {
        var loadNewScene = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        
        while (!loadNewScene.isDone)
        {
            yield return null;
        }
        
        SceneManager.UnloadSceneAsync(oldScene);
    }
    
    IEnumerator LoadSceneAsynchronously(Scene oldScene, string newScene)
    {
        Debug.Log("loadign scene!!!");
        var loadNewScene = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        
        while (!loadNewScene.isDone)
        {
            yield return null;
        }
        
        SceneManager.UnloadSceneAsync(oldScene);
    }

    private void Start()
    {
        playerLocation = "Lobby";
        LoadGame();
       // MovePlayer(playerLocation);
    }

    private struct CurrentConversationValues
    {
        internal static string Title = string.Empty;
        internal static string Actor;
        internal static string Conversant;
        internal static int LineID;
        internal static string Trigger;
    }

    public static string MostRecentResponseNode = string.Empty;
    public static string MostRecentDialogueTrigger = string.Empty;

    private void ExecutePlayerEvent(PlayerEvents.PlayerEvent eventFromStack)
    {
        var type = eventFromStack.Type;
        var sender = eventFromStack.Sender;
        var receiver = eventFromStack.Receiver;
        var value = eventFromStack.Value;

        //related to conversation_start and conversation_end events
        string conversationCycle = $"{receiver}_cycle";
        int cycle = DialogueLua.GetVariable(conversationCycle, 0);
        
        switch (type)
        {
            case "move":
                if (receiver == "player")
                {
                   // 
                    var newScene = $"Resources/Scenes/{value}";
                    var oldScene = $"Resources/Scenes/{playerLocation}";
                    playerLocation = value;
                    

                    if (gameIsLoading) return;
                    Debug.Log("loading scene!! :)");
                    StartCoroutine(LoadSceneAsynchronously(oldScene, newScene));
                    
                }
                break;
            case "interact":
                var interact = ($"${sender}_interact_{receiver}"); // e.g. "player_interact_VendingMachine"
                var interactCount = DialogueLua.GetVariable($"player_interact_{receiver}", 0);
                DialogueLua.SetVariable($"player_interact_{receiver}", interactCount + 1);
                MostRecentDialogueTrigger = value == "DialogueSystemTrigger" ? receiver : string.Empty;
                break;
            case "conversation_start": 
                // if the conversation is starting, set the current conversation title to the conversation title
                CurrentConversationValues.Title = receiver;
                // if the conversation has not played before, set the cycle value to 0
                if (cycle == 0) DialogueLua.SetVariable(conversationCycle, cycle); 
                break;
            case "conversation_end":
                cycle = DialogueLua.GetVariable(conversationCycle, 0);
                // if the conversation is ending, set the current conversation title to empty
                // increment the cycle value
                DialogueLua.SetVariable(conversationCycle, cycle + 1);
                CurrentConversationValues.Title = CurrentConversationValues.Actor = CurrentConversationValues.Conversant = string.Empty;
                break;
            case "awaiting_response":
                MostRecentResponseNode = sender;
                break;
            case "conversation_decision":
                DialogueLua.SetVariable($"{CurrentConversationValues.Title}_{MostRecentResponseNode}", value); // e.g. "IntroSequence_Choice1" = "Yes"
                MostRecentResponseNode = string.Empty;
                break;
            case "line":
                CurrentConversationValues.Actor = sender;
                CurrentConversationValues.Conversant = receiver;
                CurrentConversationValues.LineID = int.Parse(value);
                break;
        }
    }
    
}
