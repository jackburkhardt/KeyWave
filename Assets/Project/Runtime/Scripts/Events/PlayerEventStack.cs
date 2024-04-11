using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Project.Runtime.Scripts.App;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PlayerEvent
{
    private string PlayerID;
    public string EventType { get; }
    public string Source { get; }
    public string Target { get; } 
    public DateTime LocalTimeStamp { get; }
    public string Data { get; }

    /// <summary>
    /// An event that was triggered by a player action. Used mostly for server data logging.
    /// </summary>
    /// <param name="eventType">What happened?</param>
    /// <param name="source">Where did it come from?</param>
    /// <param name="target">Who is it directed to?</param>
    /// <param name="data">Other information relevant to this event (points, duration, etc)</param>
    public PlayerEvent(string eventType, string source, string target, params object[] data)
    {
        this.EventType = eventType;
        this.Source = source;
        this.Target = target;
        this.LocalTimeStamp = DateTime.Now;
        this.PlayerID = App.PlayerID;
        this.Data = JsonConvert.SerializeObject(data);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

[Serializable]
public class PlayerEventStack : ScriptableObject
{
    public List<PlayerEvent> RegisteredEvents { get; private set; } = new();

    private void Awake()
    {
        GameEvent.OnRegisterPlayerEvent += RegisterPlayerEvent;
    }

    private void RegisterPlayerEvent(PlayerEvent e)
    {
        RegisteredEvents.Add(e);
        GameEvent.RunPlayerEvent(e);
    }
    
    public string SerializeEvents()
    {
        return JsonConvert.SerializeObject(RegisteredEvents, Formatting.Indented);
    }
    
    public IEnumerator RunEvents()
    {
        foreach (var playerEvent in RegisteredEvents)
        {
            GameEvent.RunPlayerEvent(playerEvent);
        }
        yield return null;
    }

    private void OnDestroy()
    {
        GameEvent.OnRegisterPlayerEvent -= RegisterPlayerEvent;
    }
}





