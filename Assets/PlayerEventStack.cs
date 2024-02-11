using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]

public class PlayerEvents
{
    /*
     * This class has one field: a list of PlayerEvent objects.
     * A PlayerEvent object has five fields: the type of event, the sender of the event, the receiver, the value, the duration, and the timestamp of the event.
     * The list of PlayerEvents is serialized and saved to a JSON file.
    */

    public List<PlayerEvent> events = new();

    [System.Serializable]
    public class PlayerEvent
    {
        [SerializeField] private string type;
        [SerializeField] private string sender;
        [SerializeField] private string receiver;
        [SerializeField] private string value;
        [SerializeField] private int duration;
       [SerializeField] private string timeStamp;
       [SerializeField] private string log;

        
        public string Type => type;
        public string Sender => sender;
        public string Receiver => receiver;
        public string Value => value;
        public int Duration => duration;

        public PlayerEvent(string type, string sender, string receiver, string value, int duration = 0, string log = "",  string timeStamp = null)
        {
            this.value = value;
            this.type = type;
            this.sender = sender;
            this.receiver = receiver;
            this.duration = duration * GameManager.TimeScales.GlobalTimeScale;
            this.log = log;
            this.timeStamp = System.DateTime.Now.ToString();
        }
    }
    public int Count() => events.Count;
}

public class PlayerEventStack : JsonSerializer
{
    public static PlayerEvents eventsWrapper;

    private static PlayerEventStack instance;
 
    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        else if (instance != this)
        {
            Destroy(this);
        }
        
        FileName =  "PlayerEventStack.json";
    }
    
    protected override void OnSerializePlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        eventsWrapper.events.Add(playerEvent);
        Serialize(eventsWrapper);
        GameEvent.RunPlayerEvent(playerEvent);
    }
    
    public IEnumerator RunEvents() //called by GameManager
    {
        foreach (var playerEvent in eventsWrapper.events)
        {
            GameEvent.RunPlayerEvent(playerEvent);
        }
        
        yield return null;
        
    }

    protected override void OnPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        
    }

    protected override void OnLoad()
    {
       Deserialize(ref eventsWrapper);
    }
}





