using PixelCrushers.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameData
{
    public string player_location;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerEventStack playerEventStack;
    //chapter
    public static string currentModule;
    public static string currentChapter;
    //actor
    public static string playerLocation;

    public static bool isControlEnabled = true;

    public static Canvas mainOverlayCanvas;

    [SerializeField] Canvas overlayCanvas;


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

    // Start is called before the first frame update
    private void Awake()
    {
        currentModule = module.ToString();
        currentChapter = chapter.ToString();

        StartCoroutine(SetPath());
        IEnumerator SetPath()
        {
            while (GameManager.currentModule == null) yield return null;
            _path = $"{Application.dataPath}/Resources/GameData/{GameManager.currentModule}/Game.json";
        }

        mainOverlayCanvas = overlayCanvas;

    }

    private void OnEnable()
    {
        GameEvent.OnPlayerEvent += InterpretEvent;
    }

    bool gameIsLoading;

    private void LoadGame()
    {
        gameIsLoading = true;

        for (int i = 0; i < playerEventStack.events.Count(); i++)
        {
            InterpretEvent(playerEventStack.events.GetTypeFromIndex(i), playerEventStack.events.GetInputFromIndex(i));
        }

        gameIsLoading = false;

        MovePlayer(playerLocation);
    }

    private void Start()
    {
        playerLocation = "Lobby";
        LoadGame();
        MovePlayer(playerLocation);
    }

    private void InterpretEvent(string eventType, string eventValue)
    {
        switch (eventType)
        {
            case "player_enter":
                MovePlayer(eventValue);
                break;
            case "player_interact":
                PlayerInteract(eventValue);
                break;
            case "conversation_start":
                ConversationStart(eventValue);
                break;
            case "conversation_decision":
                ConversationDecision(eventValue);
                break;
            case "conversation_end":
                ConversationEnd(eventValue);
                break;
        }
    }
    private void ConversationStart(string eventValue)
    {
        var conversation = eventValue;
        DialogueLua.SetVariable($"{conversation}_cycle", DialogueLua.GetVariable($"{conversation}_cycle", 0)); //create variable if it doesn't exist
        DialogueLua.SetVariable("current_conversation_cycle", DialogueLua.GetVariable($"{conversation}_cycle").asInt);

        Debug.Log($"Conversation {conversation} started. Cycle: {DialogueLua.GetVariable($"{conversation}_cycle").asInt}");
    }

    private void ConversationEnd(string eventValue)
    {
        var conversation = eventValue;
        DialogueLua.SetVariable("current_conversation_cycle", null);
        DialogueLua.SetVariable($"{conversation}_cycle", DialogueLua.GetVariable($"{conversation}_cycle").asInt + 1);

       Debug.Log($"Conversation {conversation} ended. Cycle: {DialogueLua.GetVariable($"{conversation}_cycle").asInt}");
    }

    private void ConversationDecision(string eventValue)
    {
        string[] parsedEvent = eventValue.Split('_');

        if (parsedEvent.Length != 3) throw new System.Exception("Conversation Decision event value is not formatted correctly. Make sure there are exactly three instances of '_'.");

        DialogueLua.SetVariable($"{parsedEvent[0]}_{parsedEvent[1]}", parsedEvent[2]);
    }

    private void PlayerInteract(string eventValue)
    {
        DialogueLua.SetVariable($"player_interact_{eventValue}", DialogueLua.GetVariable($"player_interact_{eventValue}", 0) + 1);
    }



    public void MovePlayer(string destination)
    {
        playerLocation = destination;

        if (gameIsLoading) return;

        SetCamera(GameObject.Find(destination).transform);
    }

    public void SetCamera(Transform m_transform)
    {
        // grab a gameobject that matches the string, and then move the camera to that position and set the orthographic size to the size of the object

        Camera.main.transform.position = new Vector3(m_transform.position.x, m_transform.position.y, -10);

        Camera.main.orthographicSize = mainOverlayCanvas.GetComponent<CanvasScaler>().referenceResolution.y * m_transform.localScale.x / 2;
    }
}
