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
        [SerializeField] private string sender;
        [SerializeField] private string receiver;
        [SerializeField] private string value;
        [SerializeField] private string timeStamp;
        
        public string Type => type;
        public string Sender => sender;
        public string Receiver => receiver;
        public string Value => value;

        public PlayerEvent(string type, string sender, string receiver, string value, string timeStamp = null)
        {
            this.value = value;
            this.type = type;
            this.sender = sender;
            this.receiver = receiver;
            this.timeStamp = System.DateTime.Now.ToString();
        }
        
    }

    public int Count() => events.Count;

}

public class PlayerEventStack : MonoBehaviour
{
    
   
    /*
     * This class creates a stack of events that the player has done.
     * When loading the game, the stack is read and the events are played back in order to resume the game to the correct state.
     */

    public bool verboseLogging = true;
    public static PlayerEvents eventsWrapper;
    string _path;


    private void AddEvent(PlayerEvents.PlayerEvent playerEvent)
    {
       eventsWrapper.events.Add(playerEvent);
       DataManager.SerializeData(eventsWrapper, _path);
    }

    private void OnEnable()
    {

        _path = $"{Application.dataPath}/Resources/GameData/{GameManager.currentModule}/PlayerEventStack.json";
        eventsWrapper = DataManager.DeserializeData<PlayerEvents>(_path);
        Debug.Log(eventsWrapper);

        GameEvent.OnPlayerEvent += AddEvent;

    }

    private void OnDisable()
    {
        GameEvent.OnPlayerEvent -= AddEvent;
    }
    
    
    
    //INVOKE EVENTS 

    
    
    /*

    public void OnConversationEnd() { 
    
        var conversation = DialogueManager.instance.activeConversation.conversationTitle;
        GameEvent.PlayerEvent("dialogue_end", conversation);

    }
    
    */
}
