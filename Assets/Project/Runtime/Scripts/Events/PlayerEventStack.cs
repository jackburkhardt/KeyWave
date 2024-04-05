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
    private string log;
    private string playerID;
    public string Type { get; }
    public string Source { get; }
    public string Target { get; }
    public string Value { get; }
    public int Duration { get; }
    public DateTime TimeStamp { get; }

    public PlayerEvent(string type, string source, string target, string value, int duration = 0, string log = "")
    {
        this.Value = value;
        this.Type = type;
        this.Source = source;
        this.Target = target;
        this.Duration = duration;
        this.log = log;
        this.TimeStamp = DateTime.Now;
        this.playerID = App.PlayerID;
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





