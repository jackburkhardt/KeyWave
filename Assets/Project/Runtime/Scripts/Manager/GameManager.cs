using System;
using System.Collections;
using System.Collections.Generic;
using KeyWave.Runtime.Scripts.AssetLoading;
using Language.Lua;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameStateManager gameStateManager;
    public PlayerEventStack playerEventStack;
    private CustomLuaFunctions _customLuaFunctions;
    public List<Location> locations;
    public bool capFramerate = false;
    public DailyReport dailyReport;
    
    [ShowIf("capFramerate")]
    public int framerateLimit;
    
    public static GameState gameState => GameStateManager.instance.gameState;

    // Start is called before the first frame update

    private void OnEnable()
    {
       
    }

    public void PauseDialogueSystem()
    {
        DialogueManager.instance.Pause();
        WatchHandCursor.GlobalFreeze();
    }
    
    public void UnpauseDialogueSystem()
    {
        DialogueManager.instance.Unpause();
        WatchHandCursor.GlobalUnfreeze();
    }


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
        
        if (playerEventStack == null)
        {
            playerEventStack = ScriptableObject.CreateInstance<PlayerEventStack>();
        }
        _customLuaFunctions = GetComponent<CustomLuaFunctions>() ?? gameObject.AddComponent<CustomLuaFunctions>();

    }
  
    private void Start()
    {
        GameEvent.OnPlayerEvent += OnPlayerEvent;
        //OnSaveDataApplied();
    }

    public void OnSaveDataApplied()
    {
        dailyReport ??= new DailyReport(gameState.day);

        FindObjectOfType<PointsBar>().ApplySaveData();
    }
    
    private void OnPlayerEvent(PlayerEvent e)
    {
        if (gameState.Clock > Clock.DailyLimit)
        {
            GameEvent.OnDayEnd();
            DialogueManager.StopAllConversations();
            EndOfDay();
        }
    }

    public void StartNewDay()
    {
        GameStateManager.instance.StartNextDay();
        dailyReport = new DailyReport(gameState.day);
        App.Instance.ChangeScene("Hotel", "EndOfDay");
    }

    public IEnumerator StartNewSave()
    {
        yield return App.Instance.LoadScene("Hotel");
        dailyReport = new DailyReport(gameState.day);
        DialogueManager.StartConversation("Intro");
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
    
    
    public static void OnQuestStateChange(string questName)
    {
        var quest = DialogueUtility.GetQuestByName(questName);
        var state = QuestLog.GetQuestState(questName);
        var points = DialogueUtility.GetPointsFromField(quest.fields);

        if (state == QuestState.Success && points.Points > 0)
        {
            GameEvent.OnPointsIncrease(points, questName);
        }
        
        var duration = DialogueUtility.GetQuestDuration(quest);
        
        /*
        
        if (state == QuestState.Success && duration > 0)
        {
            gameState.Clock += duration;
        }
        
        */
        
        GameEvent.OnQuestStateChange(questName, state, duration);
    }

    private string last_convo = string.Empty;
    public void OpenMap()
    {
        //last_convo = gameState.current_conversation_title;
        DialogueManager.StopConversation();
        DialogueManager.instance.PlaySequence("SetDialoguePanel(false)");
        App.Instance.LoadScene("Map");
    }

    public void CloseMap(bool returnToConvo)
    {
        App.Instance.UnloadScene("Map");
        if (returnToConvo)
        {
            DialogueManager.instance.PlaySequence("SetDialoguePanel(true)");
            DialogueManager.StartConversation(last_convo);
        }
    }


    public void EndOfDay() => App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene);

    public void StartOfDay() => App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);

    public void TravelTo(Location location)
    {
        TravelTo(location.Name);
    }
    
    public void TravelTo(string newLocation, string currentScene = "")
    {

        DialogueManager.StopConversation();
        if (currentScene == "") currentScene = gameState.current_scene;
        StartCoroutine(TravelToHandler());

        IEnumerator TravelToHandler()
        {
            yield return App.Instance.ChangeScene(newLocation, currentScene);
            
            yield return new WaitForSeconds(0.5f);

            DialogueManager.StartConversation($"{newLocation}/Base");

            gameState.current_scene = newLocation;
        }
    }
    
    public void Wait(int duration)
    {
       GameEvent.OnWait(duration);
    }

    private void Update()
    {
        if (capFramerate) Application.targetFrameRate = framerateLimit;
    }
}
