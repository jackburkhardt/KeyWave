using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class GameLog : MonoBehaviour
{
    public enum LogType
    {
        Default,
        Travel,
        Quest,
        Points,
        PhoneCall
    }

    private void OnEnable()
    {
        GameEvent.OnPlayerEvent += OnPlayerEvent;
        PhoneCallPanel.OnPhoneCallStart += LogPhoneCallStart;
        PhoneCallPanel.OnPhoneCallEnd += LogPhoneCallEnd;
        LocationManager.OnLocationEnter += OnLocationEnter;
        LocationManager.OnLocationExit += OnLocationLeave;
    }
    
    private void OnDisable()
    {
        GameEvent.OnPlayerEvent -= OnPlayerEvent;
        PhoneCallPanel.OnPhoneCallStart -= LogPhoneCallStart;
        PhoneCallPanel.OnPhoneCallEnd -= LogPhoneCallEnd;
        LocationManager.OnLocationEnter -= OnLocationEnter;
        LocationManager.OnLocationExit -= OnLocationLeave;
    }

    private void OnPlayerEvent(PlayerEvent e) 
    {
        if (e.EventType == "quest_state_change" && e.Data["state"].ToString() == "Success")
        {
            OnQuestComplete(e.Data["questName"].ToString());
        }
        
        if (e.EventType == "action_state_change" && e.Data["state"].ToString() == "Success")
        {
            OnActionComplete(e.Data["actionName"].ToString());
        }
    }
    
    

    public static void Log(string message, LogType type = LogType.Default)
    {
        
        var log = DialogueLua.GetVariable("game.log").asString;
        
        var color = type switch
        {
            LogType.Default => string.Empty,
            LogType.Travel => "<color=#98fff3>",
            LogType.Quest => "<color=#F9E076>",
            LogType.Points => "<color=#78AB78>",
            LogType.PhoneCall => "<color=#78AB78>",
            _ => string.Empty
        };
            
            
        log += $"{color}[{Clock.CurrentTime}] {color}{message}<br>";
        DialogueLua.SetVariable("game.log", log);
    }
    
    
    public static void ClearLog()
    {
        DialogueLua.SetVariable("game.log", string.Empty);
    }
  
    public static void LogQuest(string message)
    {
        Log(message, LogType.Quest);
    }
    
    public static void LogTravel(string message)
    {
        
        Log(message, LogType.Travel);
    }
    
    public static void LogPoints(string message)
    {
        Log(message, LogType.Points);
    }
    
    public static void LogPhoneCallStart(string contactName)
    {
        Log($"Phone Call: {contactName}.", LogType.PhoneCall);
    }
    
    public static void LogPhoneCallEnd(string contactName)
    {
        Log($"Call end.", LogType.PhoneCall);
    }
    
    public void OnLocationLeave(Location location)
    {
        var message = $"Left {location.Name}";
        LogTravel(message);
    }
    
    public void OnLocationEnter(Location location)
    {
        var message = $"Travelled to {location.Name}";
        LogTravel(message);
    }

    
    public void OnQuestComplete(string questName)
    {
        var quest = QuestLog.GetQuestDescription(questName);
        var message = $"{quest}";
        LogQuest(message);
    }
    
    public void OnActionComplete(string actionName)
    {
        var desc = DialogueLua.GetItemField(actionName, "Success Description").asString;
        if (string.IsNullOrEmpty(desc)) return;
        LogQuest(desc);
    }
    
    public static IEnumerable<string> GetLogWithoutFormatting()
    {
        var log = DialogueLua.GetVariable("game.log").asString;
        var logEntries = log.Split("<br>")[1..];
        for (int i = 0; i < logEntries.Length - 1; i++)
        {
            logEntries[i] = Regex.Replace(logEntries[i], @"<[^>]*>", string.Empty);
        }

        return logEntries.Where(e => !string.IsNullOrEmpty(e));
    }

    public static JObject Log2Json()
    {
        var entries = GetLogWithoutFormatting();
        
        // for splitting timestamp from actual event
        Dictionary<string, string> logAsDict = new();
        foreach (var entry in entries)
        {
            var time = entry[1..6].Replace(':', '_');
            var message = entry[8..];
            
            logAsDict.Add(time, message);
        }

        JObject log = new JObject
        {
            ["GameLog"] = JObject.FromObject(logAsDict)
        };
        
        return log;
    }
}
