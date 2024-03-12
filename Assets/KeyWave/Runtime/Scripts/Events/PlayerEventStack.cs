using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerEvent
{
    [SerializeField] private string type;
    [SerializeField] private string sender;
    [SerializeField] private string receiver;
    [SerializeField] private string value; 
    [SerializeField] private int duration;
    [SerializeField] private DateTime timeStamp;
    [SerializeField] private string log;

        
    public string Type => type;
    public string Sender => sender;
    public string Receiver => receiver;
    public string Value => value;
    public int Duration => duration;
    public DateTime TimeStamp => timeStamp;

    public PlayerEvent(string type, string sender, string receiver, string value, int duration = 0, string log = "")
    {
        this.value = value;
        this.type = type;
        this.sender = sender;
        this.receiver = receiver;
        this.duration = duration;
        this.log = log;
        this.timeStamp = DateTime.Now;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

public class PlayerEventStack : ScriptableObject
{
    private static PlayerEventStack instance;

    public List<PlayerEvent> RegisteredEvents { get; private set; } = new();

    private void Awake()
    {
        GameEvent.OnPlayerEvent += RegisterPlayerEvent;
    }

    private void RegisterPlayerEvent(PlayerEvent e)
    {
        RegisteredEvents.Add(e);
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
        GameEvent.OnPlayerEvent -= RegisterPlayerEvent;
    }
}





