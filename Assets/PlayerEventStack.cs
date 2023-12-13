using System.Collections;
using System.Collections.Generic;
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

    public List<PlayerEvent> events;

    public PlayerEvents()
    {
        events = new List<PlayerEvent>();
    }

    [System.Serializable]
    public class PlayerEvent
    {
        /*
         * Event types include: player_interact, player_move, lua_decision
         */

        [SerializeField] string EventType;
        [SerializeField] string EventValue;
        [SerializeField] string Timestamp;

        public PlayerEvent(string eventType, string eventValue, string timestamp)
        {
            EventType = eventType;
            EventValue = eventValue;
            Timestamp = timestamp;
        }

        // for readability purposes

        public string GetEventType()
        {
            return EventType;
        }

        public string GetEventValue()
        {
            return EventValue;
        }

    }

    public void AddEvent(string eventType, string eventValue, bool logToConsole = false)
    {
        events.Add(new PlayerEvent(eventType, eventValue, System.DateTime.Now.ToString()));

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


    private void AddEvent(string eventType, string eventInput)
    {
       events.AddEvent(eventType, eventInput, verboseLogging);
       
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
