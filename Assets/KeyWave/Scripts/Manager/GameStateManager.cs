using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


#region  deprecated

/*

public class GameState
{
    public int clock = 21600000;
    public string player_location = "Lobby";
    public string current_conversation_title = string.Empty;
    public string current_conversation_actor;
    public string current_conversation_conversant;
    public int current_conversation_line;
    public string most_recent_response_node = string.Empty;
    public List<(string, object)> lua_variables = new List<(string, object)>();
    public List<string> lua_scripts = new List<string>();
    
    

    public void AddLuaVariable(string variableName, object value, bool overwrite = true)
    {
        bool variableExists = false;

        foreach (var tuple in lua_variables)
        {
            var luaVariable = tuple;
            if (luaVariable.Item1 == variableName)
            {
                if (overwrite) luaVariable.Item2 = value;
                variableExists = true;
                break;
            }
        }

        if (!variableExists) lua_variables.Add((variableName, value));
        
        if (overwrite) DialogueLua.SetVariable(variableName, value);
    }

    public void IncrementLuaVariable(string variableName)
    {
        foreach (var tuple in lua_variables)
        {
            var luaVariable = tuple;
            if (luaVariable.Item1 == variableName)
            {
                var value = (int)luaVariable.Item2;
                luaVariable.Item2 = (value + 1);
                break;
            }
        }
        
        DialogueLua.SetVariable(variableName, DialogueLua.GetVariable(variableName).asInt + 1);
        
    }
}

public class GameStateManager : PlayerEventHandler
{
    [SerializeField] private int clock = 21600000;
    [SerializeField] private string player_location = "Lobby";
    [SerializeField] private string current_conversation_title = string.Empty;
    [SerializeField] private string current_conversation_actor;
    [SerializeField] private string current_conversation_conversant;
    [SerializeField] private int current_conversation_line;
    [SerializeField] private string most_recent_response_node = string.Empty;
    [SerializeField] private List<(string, object)> lua_variables = new List<(string, object)>();
    
    
    private static GameStateManager instance;

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
    protected override void OnPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
       switch (playerEvent.Type)
            {
                case "move":
                    gameState.player_location = playerEvent.Value;
                    break;
                case "conversation_start":
                    gameState.current_conversation_title = playerEvent.Value;
                 //   gameState.AddLuaVariable($"{playerEvent.Value}_cycle", 0, false);
                    break;
                case "conversation_end":
                   // gameState.IncrementLuaVariable($"{playerEvent.Value}_cycle");
                    break;
                case "conversation_script":
                    gameState.lua_scripts.Add(playerEvent.Value);
                    break;
                case "conversation_line":
                    gameState.current_conversation_actor = playerEvent.Sender;
                    gameState.current_conversation_conversant = playerEvent.Receiver;
                    gameState.current_conversation_line = int.Parse(playerEvent.Value);
                    break;
                case "awaiting_response":
                    gameState.most_recent_response_node = playerEvent.Value;
                    break;
                case "conversation_decision":
                    gameState.most_recent_response_node = string.Empty;
                   // gameState.AddLuaVariable($"{gameState.current_conversation_title}_{gameState.most_recent_response_node}", playerEvent.Value);  // e.g. "IntroSequence_Choice1" = "Yes"
                    break;
            }
    }

    public void LoadGameState()
    {
        //load scenes
        //SceneManager.LoadScene("Resources/Scenes/Base");
        SceneManager.LoadSceneAsync($"Resources/Scenes/{gameState.player_location}", LoadSceneMode.Additive);
        
        /*
        
        foreach (var conversation in DialogueManager.masterDatabase.conversations)
        {
            gameState.AddLuaVariable($"{conversation.Title}_cycle", 0, false);
        }
        
        */
        
        //set lua variables
        
        /*

        foreach (var lua_variable in gameState.lua_variables)
        {
            DialogueLua.SetVariable(lua_variable.Item1, lua_variable.Item2);
        }
        
  

        foreach (var script in gameState.lua_scripts)
        {
            Lua.Run(script);
        }
        
        //load conversation -- order matters here
        if (gameState.current_conversation_title != string.Empty)
        {
            StartCoroutine(StartConversationHandler());
        }
        
        else DialogueManager.StartConversation(gameState.player_location);
        
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

    private void Update()
    {
        player_location = gameState.player_location;
        clock = gameState.clock;
        current_conversation_title = gameState.current_conversation_title;
        current_conversation_actor = gameState.current_conversation_actor;
        current_conversation_conversant = gameState.current_conversation_conversant;
        current_conversation_line = gameState.current_conversation_line;
        most_recent_response_node = gameState.most_recent_response_node;
        lua_variables = gameState.lua_variables;
        
    }
}

*/

#endregion

public class GameState
{
    public int clock = 21600;
    public string player_location = "Hotel";
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
    protected override void OnPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
       switch (playerEvent.Type)
            {
                case "move":
                    gameState.player_location = playerEvent.Value;
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
            }
       
        gameState.clock += playerEvent.Duration;
        DialogueLua.SetVariable("clock", gameState.clock);

        foreach (var location in GameManager.instance.locations)
        {
            DialogueLua.SetLocationField(location.name, "ETA", GameManager.instance.GetEtaToLocation(location.location));
        }
        
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
        
        //load conversation -- order matters here
        if (gameState.current_conversation_title != string.Empty)
        {
            StartCoroutine(StartConversationHandler());
        }
        
        else DialogueManager.StartConversation($"{gameState.player_location}/Base");
        
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


