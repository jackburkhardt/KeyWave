using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using PixelCrushers.DialogueSystem;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]

public class PlayerEvents
{
    /*
     * This class has one field: a list of PlayerEvent objects.
     * A PlayerEvent object has three fields: the type of event, the input of the event, and the timestamp of the event.
     * The list of PlayerEvents is serialized and saved to a JSON file.
    */

    public List<PlayerEvent> events = new();

    [System.Serializable]
    public class PlayerEvent
    {
        /*
         * Event types include: player_interact, player_move, lua_decision
         */

        [SerializeField] private string type;
        [SerializeField] private string index;
        [SerializeField] private string value;
        [SerializeField] private string sender;
        [SerializeField] private string receiver;
        [SerializeField] private string timeStamp;

        public PlayerEvent(string eventType, string eventValue, string eventSender, string eventReceiver, string timestamp)
        {
            this.value = eventValue;
            this.type = eventType;
            this.sender = eventSender;
            this.reciever = eventReceiver;
            timeStamp = timestamp;
        }

        // for readability purposes

        public string GetEventType()
        {
            return type;
        }

        public string GetEventValue()
        {
            return value;
        }
    }

    public void AddEvent(string eventType, string eventSender, string eventReceiver, string eventValue, bool logToConsole = false)
    {
        events.Add(new PlayerEvent(eventType, eventSender, eventReceiver, eventValue,System.DateTime.Now.ToString(CultureInfo.CurrentCulture)));

        if (logToConsole) Debug.Log($"Added event: {eventType} {eventValue}");
    }

    public string GetTypeFromIndex(int index) => events[index].GetEventType();

    public string GetInputFromIndex(int index) => events[index].GetEventValue();

    public int Count() => events.Count;

}




public class PlayerEventStack : MonoBehaviour
{
    /*
     * This class creates a stack of events that the player has done.
     * When loading the game, the stack is read and the events are played back in order to resume the game to the correct state.
     */

    public bool verboseLogging = true;

    public PlayerEvents events;
    string _path;


    private void AddEvent(string eventType, string eventSender, string eventReceiver, string eventValue)
    {
       events.AddEvent(eventType, eventValue, eventSender, eventReceiver);
       
       DataManager.SerializeData(events, _path);
    }

    private void OnEnable()
    {

        _path = $"{Application.dataPath}/Resources/GameData/{GameManager.currentModule}/PlayerEventStack.json";
        events = DataManager.DeserializeData<PlayerEvents>(_path);
        Debug.Log(events);

        GameEvent.OnPlayerEvent += AddEvent;

    }

    private void OnDisable()
    {
        GameEvent.OnPlayerEvent -= AddEvent;
    }
    
    
    
    //INVOKE EVENTS 

    public void OnInteraction(string receiver)
    {
        GameEvent.PlayerEvent("interact", "player", receiver);
    }

    public void OnConversationDecision(string conversation, string decision)
    {
        var sender = DialogueManager.instance.activeConversation.conversationTitle;
        var currentNode = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.Title;
        GameEvent.PlayerEvent("decision", conversation, currentNode, decision);
    }

    public void OnConversationState(string state)
    {
        var conversation = DialogueManager.instance.activeConversation.conversationTitle;
        GameEvent.PlayerEvent("dialogue",  conversation, state);
    }

    public void OnConversationLine()
    {
        var actor;
        var conversant;
        var 
    }
    
    /*

    public void OnConversationEnd() { 
    
        var conversation = DialogueManager.instance.activeConversation.conversationTitle;
        GameEvent.PlayerEvent("dialogue_end", conversation);

    }
    
    */
}
