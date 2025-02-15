using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

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
        QuestUtility.OnQuestComplete += OnQuest;
    }
    
    private void OnDisable()
    {
        Points.OnPointsChange -= OnPoints;
        QuestUtility.OnQuestComplete -= OnQuest;
    }

    public static void Log(string message, LogType type = LogType.Default)
    {
        
        var log = DialogueLua.GetVariable("game.log").asString;
        
        if (log.Contains("[temp]"))
        {
            log = string.Empty;
        }
        
        var color = type switch
        {
            LogType.Default => string.Empty,
            LogType.Travel => "<color=#98fff3>",
            LogType.Quest => "<color=#F9E076>",
            LogType.Points => "<color=#78AB78>",
            _ => string.Empty
        };
            
            
        log += $"[br({color}{Clock.CurrentTime})] {color}{message}";
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
}
