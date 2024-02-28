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
    public List<GameLocation> locations;
    public List<GameObjective> Objectives;

    public static GameState gameState;
    
    public struct TimeScales
    {
        internal static int GlobalTimeScale = 15;
        internal static int SpokenCharactersPerSecond = 5;
        internal static int SecondsBetweenLines = 15;
        internal static int SecondsPerInteract = 30;
    }

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
        

        foreach (var location in locations)
        {
            DialogueLua.SetLocationField(location.name, "X Coordinate", location.coordinates.x);
            DialogueLua.SetLocationField(location.name, "Y Coordinate", location.coordinates.y);
            DialogueLua.SetLocationField(location.name, "Description", location.description);
            
        }

    }

   
    

    public enum Region {
        Vitoria,
        Recife
    }

    public enum Locations {
        Hotel,
        Beach,
        Airport,
        Mall,
        Park,
        Island,
        Café,
        Store
    }
    
    public static GameLocation GetGameLocation(Locations location) => GetGameLocation(location.ToString());
    
    public static GameLocation GetGameLocation(string location)
    {
        GameLocation gameLocation = null;
        
        foreach (var loc in instance.locations)
        {
            if (loc.name != location) continue;
            gameLocation = loc;
            break;
        }
        return gameLocation;
    }
    
    public static List<GameLocation.Objectives> GetLocationObjectives(string location)
    {
        var gameLocation = GetGameLocation(location);
        return gameLocation.objectives;
    }

    public static List<GameLocation.Objectives> GetLocationObjectives(Locations location) => GetLocationObjectives(location.ToString());
  
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
    
    
    public enum Hour
    {
        _0 = 0,
        _1 = 1,
        _2 = 2,
        _3 = 3,
        _4 = 4,
        _5 = 5,
        _6 = 6,
        _7 = 7,
        _8 = 8,
        _9 = 9,
        _10 = 10,
        _11 = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
        _16 = 16,
        _17 = 17,
        _18 = 18,
        _19 = 19,
        _20 = 20,
        _21 = 21,
        _22 = 22,
        _23 = 23
    }

    public enum Minute
    {
        _00 = 0,
        _01 = 1,
        _02 = 2,
        _03 = 3,
        _04 = 4,
        _05 = 5,
        _06 = 6,
        _07 = 7,
        _08 = 8,
        _09 = 9,
        _10 = 10,
        _11 = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
        _16 = 16,
        _17 = 17,
        _18 = 18,
        _19 = 19,
        _20 = 20,
        _21 = 21,
        _22 = 22,
        _23 = 23,
        _24 = 24,
        _25 = 25,
        _26 = 26,
        _27 = 27,
        _28 = 28,
        _29 = 29,
        _30 = 30,
        _31 = 31,
        _32 = 32,
        _33 = 33,
        _34 = 34,
        _35 = 35,
        _36 = 36,
        _37 = 37,
        _38 = 38,
        _39 = 39,
        _40 = 40,
        _41 = 41,
        _42 = 42,
        _43 = 43,
        _44 = 44,
        _45 = 45,
        _46 = 46,
        _47 = 47,
        _48 = 48,
        _49 = 49,
        _50 = 50,
        _51 = 51,
        _52 = 52,
        _53 = 53,
        _54 = 54,
        _55 = 55,
        _56 = 56,
        _57 = 57,
        _58 = 58,
        _59 = 59
    }
  
    private void Start()
    {
        StartCoroutine(StartHandler());
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

    public GameObjective GetObjectiveFromTitle(string title)
    {
        GameObjective objective = null;

        foreach (var obj in GameManager.instance.Objectives)
        {
            if (obj.objectiveTitle == title)
            {
                objective = obj;
                break;
            }
        }
        return objective;
    }

    public void UpdateObjectiveState(string objectiveTitle)
    {
        var objective = GetObjectiveFromTitle(objectiveTitle);
        var status = DialogueLua.GetQuestField(objectiveTitle, "state").ToString();
        
        objective.state = (GameObjective.State)Enum.Parse(typeof(GameObjective.State), status);
    }

    public void LoadScene(string sceneName) => StartCoroutine(LoadSceneHandler(sceneName));
    
    public void ChangeScene(string currentScene, string newScene) => StartCoroutine(LoadSceneHandler(newScene, currentScene));

    public void OpenMap() => ChangeScene(gameStateManager.gameState.player_location, "Map");
    
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

    public int GetPlayerDistanceFromLocation(Locations location)
    {
        GameLocation playerLocation = null, nextLocation = null;
        
        foreach (var gameLocation in locations)
        {
            playerLocation = gameLocation.location.ToString() == gameStateManager.gameState.player_location
                ? gameLocation
                : playerLocation;
            
            nextLocation = gameLocation.location == location
                ? gameLocation
                : nextLocation;
        }
        
        if (playerLocation == null || nextLocation == null) return 0; 
        return (int)Vector2.Distance(playerLocation.coordinates, nextLocation.coordinates);
    }
    
    public string GetEtaToLocation(Locations location)
    {
        var distance = GetPlayerDistanceFromLocation(location);
        return Clock.To24HourClock(distance * TimeScales.GlobalTimeScale + Clock.CurrentTimeRaw);
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
    
    /*


    IEnumerator StartConversationAfterOneFrame()
    {
        var actor = string.IsNullOrEmpty(CurrentConversationValues.Actor)
            ? null
            : GameObject.Find(CurrentConversationValues.Actor);
        var conversant = string.IsNullOrEmpty(CurrentConversationValues.Conversant)
            ? null
            : GameObject.Find(CurrentConversationValues.Conversant);
        var actorTransform = (actor != null) ? actor.transform : null;
        var conversantTransform = (conversant != null) ? conversant.transform : null;
        yield return new WaitForEndOfFrame();
        MostRecentDialogueTrigger = "GameManager";
        DialogueManager.StartConversation(CurrentConversationValues.Title, actorTransform, conversantTransform, CurrentConversationValues.LineID);
    }


    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator LoadGameFromStart()
    {

        while (PlayerEventStack.eventsWrapper == null) yield return null;

        DialogueLua.SetVariable("CurrentConversation", string.Empty);

        GameEvent.OnMove("GameManager", "Lobby");

        Debug.Log("loading " + PlayerEventStack.eventsWrapper.events.Count() + " events");
        this.gameIsLoading = true;

        for (int i = 0; i < PlayerEventStack.eventsWrapper.events.Count(); i++)
        {
           // Debug.Log("executing " + PlayerEventStack.eventsWrapper.events[i].Type + " event from " + PlayerEventStack.eventsWrapper.events[i].Sender + " to " + PlayerEventStack.eventsWrapper.events[i].Receiver + " with value " + PlayerEventStack.eventsWrapper.events[i].Value);
            ExecutePlayerEvent(PlayerEventStack.eventsWrapper.events[i]);
        }

        this.gameIsLoading = false;

        //load main UI
        //yield return StartCoroutine(LoadSceneAsynchronously(currentScene, playerLocation));
        // Debug.Log("starting conversation " + CurrentConversationValues.Title);s

        DialogueManager.PreloadDialogueUI();
        DialogueManager.PreloadMasterDatabase();

        if (CurrentConversationValues.Title == string.Empty)
        {
            DialogueManager.StartConversation(DialogueLua.GetVariable("player_location").asString);
        }

        else
        {
            yield return StartCoroutine(StartConversationAfterOneFrame());
        }
    }

    void LoadScene(string oldScene, string newScene)
    {
        if (gameIsLoading) return;
        StartCoroutine(LoadSceneAsynchronously(oldScene, newScene));
    }

    IEnumerator LoadSceneAsynchronously(string oldScene, string newScene)
    {
        var loadNewScene = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);

        while (!loadNewScene.isDone)
        {
            yield return null;
        }

        if (SceneManager.GetSceneByName(oldScene).isLoaded)
            SceneManager.UnloadSceneAsync(oldScene);
    }

    private void Start()
    {

       //
       // MovePlayer(playerLocation);
    }

    public static int clock = 21600000; // starts at 6am
    private int clockLimit = 86400000; // aka 24 hours



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
        var duration = eventFromStack.Duration;

        //related to conversation_start and conversation_end events
        string conversationCycle = $"{receiver}_cycle";
        int cycle = DialogueLua.GetVariable(conversationCycle, 0);

        clock = (clock + duration) % clockLimit;

        DialogueLua.SetVariable("clock", clock);

        switch (type)
        {
            case "move":
                if (receiver == "player")
                {
                    var newScene = $"Resources/Scenes/{value}";
                    var oldScene = $"Resources/Scenes/{DialogueLua.GetVariable("player_location").asString}";
                    DialogueLua.SetVariable("player_location", value);
                    playerLocation = value;
                    LoadScene(oldScene, newScene);
                }
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
                DialogueLua.SetVariable("CurrentConversation", string.Empty);
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
            case "activate_objective":
                break;
            case "wait":
                break;
        }
    }

    */
    
}
