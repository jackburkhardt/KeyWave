using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState
{
    public enum Type
    {
        Normal,
        EndOfDay
    }
    
    //default values
    
    public Type type = Type.Normal;
    private int clock = 21600;

    public int Clock
    {
        get => DialogueLua.GetVariable("clock").asInt;
        set => DialogueLua.SetVariable("clock", value);
    }
    public int day = 1;
    public int business_score = 0;  
    public int local_savvy_score = 0;
    public int wellness_score = 0;
    public string player_location = "Hotel";
    public string current_scene = "Hotel";
    public string current_conversation_title = string.Empty;
    public string current_conversation_actor;
    public string current_conversation_conversant;
    public int current_conversation_line;
    public string most_recent_response_node = string.Empty;
    public List<string> lua_scripts = new List<string>();
}

public class GameStateManager : PlayerEventHandler
{ 
    public static GameStateManager instance;
    public static Action<GameState> OnGameStateChanged;

    protected void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public GameState gameState = new GameState();
    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
       switch (playerEvent.EventType)
            {
                case "move":
                    var location = (Location)playerEvent.Data;
                    gameState.player_location = location.Name; 
                    break;
                case "conversation_start":
                    gameState.current_conversation_title = (string)playerEvent.Data;
                    break;
                case "conversation_end":
                    gameState.current_conversation_title = string.Empty;
                    break;
                case "conversation_script":
                    gameState.lua_scripts.Add((string)playerEvent.Data);
                    break;
                case "conversation_line":
                    // note: removed, this should not trigger
                    gameState.current_conversation_actor = playerEvent.Source;
                    gameState.current_conversation_conversant = playerEvent.Target;
                    gameState.current_conversation_line = (int)playerEvent.Data;
                    break;
                case "awaiting_response":
                    gameState.most_recent_response_node = (string)playerEvent.Data;
                    break;
                case "end_day":
                    gameState.type = GameState.Type.EndOfDay;
                    break;
                case "conversation_decision":
                    gameState.most_recent_response_node = string.Empty;
                    break;
                case "points":
                    var pointsField = (Points.PointsField)playerEvent.Data;
                    switch (pointsField.Type)
                    {
                        case Points.Type.Wellness:
                            gameState.wellness_score += pointsField.Points;
                            break;
                        case Points.Type.Savvy:
                            gameState.local_savvy_score += pointsField.Points;
                            break;
                        case Points.Type.Business:
                            gameState.business_score += pointsField.Points;
                            break;
                    }
                    break;
            }

            switch (gameState.type)
            {
                case GameState.Type.Normal:
                    gameState.Clock += playerEvent.Duration;
                    break;
                case GameState.Type.EndOfDay:
                    gameState.Clock = Clock.DailyLimit;
                    break;
            }
        
        OnGameStateChanged?.Invoke(gameState);
    }
    
    public void AddTime(int duration)
    {
        gameState.Clock += duration;
    }

    public void StartNextDay()
    {
        gameState.day += 1;
        gameState.Clock = 21600;
        gameState.player_location = "Hotel";
        gameState.current_conversation_title = string.Empty;
        gameState.type = GameState.Type.Normal;
    }

    public IEnumerator LoadGameState()
    {
        yield return SceneManager.LoadSceneAsync(gameState.player_location, LoadSceneMode.Additive);
        
        
        
        //run lua scripts
        foreach (var script in gameState.lua_scripts)
        {
            var parsedScript = script.Split(";");
            foreach (var line in parsedScript)
            {
                if (line.Contains("ShowAlert")) continue;
               // Lua.Run(line);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        /*
        
        //load conversation -- order matters here
        if (gameState.current_conversation_title != string.Empty)
        {
            StartCoroutine(StartConversationHandler());
        }
        
        else DialogueManager.StartConversation($"{gameState.player_location}/Base");
        
        */
        
        DialogueManager.StartConversation("Intro");
        
        IEnumerator StartConversationHandler()
        {
            var actor = string.IsNullOrEmpty(gameState.current_conversation_actor)
                ? null
                : GameObject.Find(gameState.current_conversation_actor);
            var conversant = string.IsNullOrEmpty(gameState.current_conversation_conversant)
                ? null
                : GameObject.Find(gameState.current_conversation_conversant);
            var actorTransform = (actor != null) ? actor.transform : null;
            var conversantTransform = (conversant != null) ? conversant.transform : null;
            yield return new WaitForEndOfFrame();
            DialogueManager.StartConversation(gameState.current_conversation_title, actorTransform, conversantTransform, gameState.current_conversation_line);
        }
    }

}


