using System;
using System.Collections;
using System.Collections.Generic;
using KeyWave.Runtime.Scripts.AssetLoading;
using Language.Lua;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
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
    
    public static GameState gameState;

    // Start is called before the first frame update

    private void OnEnable()
    {
       
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
        
        gameState = gameStateManager.gameState;
        
        if (playerEventStack == null)
        {
            playerEventStack = ScriptableObject.CreateInstance<PlayerEventStack>();
        }
        _customLuaFunctions = GetComponent<CustomLuaFunctions>() ?? gameObject.AddComponent<CustomLuaFunctions>();

    }
  
    private void Start()
    {
        StartCoroutine(StartHandler());
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
        App.Instance.ChangeScene("StartOfDay", gameState.current_scene);
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
    
    
    public static void OnQuestStateChange(string questName)
    {
        if (!QuestLog.IsQuestSuccessful(questName)) return;
        
        var quest = DialogueUtility.GetQuestByName(questName);
        var points = DialogueUtility.GetPointsFromField(quest.fields);
        if (points.Type == Points.Type.Null) return;
        
        GameEvent.OnPointsIncrease(points);
    }

    public void OpenMap()
    {
        DialogueManager.StopConversation();
        DialogueManager.instance.PlaySequence("SetDialoguePanel(false)");
        App.Instance.LoadScene("Map");
    }

    public void EndOfDay() => App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene);

    public void StartOfDay() => App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);

    public void TravelTo(Location location)
    {
        TravelTo(location.name);
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
    
}
