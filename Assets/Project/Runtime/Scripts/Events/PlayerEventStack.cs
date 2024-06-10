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
    public int Duration { get; }

    /// <summary>
    /// An event that was triggered by a player action. Used mostly for server data logging.
    /// </summary>
    /// <param name="eventType">What happened?</param>
    /// <param name="source">Where did it come from?</param>
    /// <param name="target">Who is it directed to?</param>
    /// <param name="data">Other information relevant to this event (point information, dialogue nodes, location details)</param>
    /// <param name="duration">How much time did this event take?</param>
    public PlayerEvent(string eventType, string source, string target, string data = "", int duration = 0)
    {
        this.EventType = eventType;
        this.Source = source;
        this.Target = target;
        this.LocalTimeStamp = DateTime.Now;
        this.PlayerID = App.PlayerID;
        this.Data = data;
        this.Duration = duration;
    }

    public override string ToString()
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
        return JsonConvert.SerializeObject(this, Formatting.Indented, settings);
    }
}

[Serializable]
public class PlayerEventStack : ScriptableObject
{
    public List<PlayerEvent> RegisteredEvents { get; private set; } = new();

    private void OnEnable()
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
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        return JsonConvert.SerializeObject(RegisteredEvents, Formatting.Indented, settings);
    }
    
    public IEnumerator RunEvents()
    {
        foreach (var playerEvent in RegisteredEvents)
        {
            GameEvent.RunPlayerEvent(playerEvent);
        }
        yield return null;
    }

    private void OnDisable()
    {
        GameEvent.OnRegisterPlayerEvent -= RegisterPlayerEvent;
    }
}





