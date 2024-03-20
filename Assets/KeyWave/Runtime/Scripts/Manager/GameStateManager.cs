using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class GameState
{
    public enum Type
    {
        Normal,
        EndOfDay
    }
    
    //default values
    
    public Type type = Type.Normal;
    public int clock = 21600;
    public int day = 1;
    public int business_score = 0;  
    public int local_savvy_score = 0;
    public int wellness_score = 0;
    public string player_location = "Hotel";
    public string player_sublocation = string.Empty;
    public string current_scene = string.Empty;
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
       switch (playerEvent.Type)
            {
                case "move":
                    var location = Location.FromString(playerEvent.Value);
                    if (location.isSublocation) gameState.player_sublocation = playerEvent.Value;
                    else { gameState.player_location = playerEvent.Value; gameState.player_sublocation = string.Empty; }
                    break;
                case "conversation_start":
                    gameState.current_conversation_title = playerEvent.Value;
                 //   gameState.AddLuaVariable($"{playerEvent.Value}_cycle", 0, false);
                    break;
                case "conversation_end":
                    gameState.current_conversation_title = string.Empty;
                   // gameState.IncrementLuaVariable($"{playerEvent.Value}_cycle");
                    break;
                case "conversation_script":
                    gameState.lua_scripts.Add(playerEvent.Value);
                    break;
                case "conversation_line":
                    // note: removed, this should not trigger
                    gameState.current_conversation_actor = playerEvent.Sender;
                    gameState.current_conversation_conversant = playerEvent.Receiver;
                    gameState.current_conversation_line = int.Parse(playerEvent.Value);
                    break;
                case "awaiting_response":
                    gameState.most_recent_response_node = playerEvent.Value;
                    break;
                case "conversation_decision":
                    gameState.most_recent_response_node = string.Empty;
                    break;
                case "state_change":
                    gameState.type = Enum.Parse<GameState.Type>(playerEvent.Value);
                    break;
                case "points":
                    var type = (Points.Type) Enum.Parse(typeof(Points.Type), playerEvent.Receiver);
                    switch (type)
                    {
                        case Points.Type.Wellness:
                            gameState.wellness_score += int.Parse(playerEvent.Value);
                            break;
                        case Points.Type.Savvy:
                            gameState.local_savvy_score += int.Parse(playerEvent.Value);
                            break;
                        case Points.Type.Business:
                            gameState.business_score += int.Parse(playerEvent.Value);
                            break;
                    }
                    break;
            }

            switch (gameState.type)
            {
                case GameState.Type.Normal:
                    gameState.clock += playerEvent.Duration;
                    break;
                case GameState.Type.EndOfDay:
                    gameState.clock = Clock.DailyLimit;
                    break;
            }
        
        OnGameStateChanged?.Invoke(gameState);
    }
    
    public void SetGameStateType(GameState.Type type)
    {
        gameState.type = type;
        switch (type)
        {
            case GameState.Type.Normal:
                break;
            case GameState.Type.EndOfDay:
                gameState.clock = Clock.DailyLimit;
                break;
        }
    }

    public void StartNextDay()
    {
        gameState.day += 1;
        gameState.clock = 21600;
        gameState.player_location = "Hotel";
        gameState.current_conversation_title = string.Empty;
        gameState.type = GameState.Type.Normal;
    }

    public IEnumerator LoadGameState()
    {
        yield return GameManager.instance.LoadSceneHandler(gameState.player_location);
        
        //run lua scripts
        foreach (var script in gameState.lua_scripts)
        {
            var parsedScript = script.Split(";");
            foreach (var line in parsedScript)
            {
                if (line.Contains("ShowAlert")) continue;
                Lua.Run(line);
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


