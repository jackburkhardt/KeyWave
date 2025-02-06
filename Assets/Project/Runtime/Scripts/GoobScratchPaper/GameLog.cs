using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

public class GameLog : MonoBehaviour
{
    public enum LogType
    {
        Default,
        Travel,
        Quest,
        Points
    }

    private void OnEnable()
    {
        Points.OnPointsChange += OnPoints;
        Location.OnLocationEnter += OnLocationEnter;
        Location.OnLocationLeave += OnLocationLeave;
        QuestUtility.OnQuestComplete += OnQuest;
    }
    
    private void OnDisable()
    {
        Points.OnPointsChange -= OnPoints;
        Location.OnLocationEnter -= OnLocationEnter;
        Location.OnLocationLeave -= OnLocationLeave;
        QuestUtility.OnQuestComplete -= OnQuest;
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

    
    public void OnQuest(Item quest)
    {
        var message = $"{quest.Description}";
        LogQuest(message);
    }
    
    public void OnPoints(Points.Type type, int amount)
    {
        if (amount < 0)
        {
            var message = $"Points: -{Math.Abs(amount)} {type}";
            LogPoints(message);
            return;
        }

        else
        {
            var message = $"Points: +{amount} {type}";
            LogPoints(message);
        }
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

    public static string SerializeForWeb()
    {
        var entries = GetLogWithoutFormatting();
        
        // for splitting timestamp from actual event
        Dictionary<string, string> logAsDict = new();
        foreach (var entry in entries)
        {
            var time = entry[1..6];
            var message = entry[8..];
            
            logAsDict.Add(time, message);
        }

        JObject log = new JObject
        {
            ["EventType"] = "player_log",
            ["Entries"] = JObject.FromObject(logAsDict)
        };
        
        return JsonConvert.SerializeObject(log, Formatting.None);
    }
}
