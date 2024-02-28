using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock
{
    public static int CurrentTimeRaw => GameManager.gameState.clock;

    public static string CurrentTime => To24HourClock(CurrentTimeRaw);
    

    public static int ToSeconds(string hoursMinutes)
    {
        if (hoursMinutes.Length != 5 || hoursMinutes.Substring(2, 1) != ":") Debug.LogError("Invalid time format");
        
        var hours = int.Parse(hoursMinutes.Substring(0, 2));
        
        var minutes = int.Parse(hoursMinutes.Substring(3, 2));
        
        return (hours * 3600 + minutes * 60);
    }

    public static string To24HourClock(int clock)
    {
        var hours = clock / 3600;
        var minutes = (clock % 3600) / 60;
        
        var minutesString = minutes < 10 ? $"0{minutes}" : minutes.ToString();
        var hoursString = hours < 10 ? $"0{hours}" : hours.ToString();
        
        return $"{hoursString}:{minutesString}";
    }
    
}
