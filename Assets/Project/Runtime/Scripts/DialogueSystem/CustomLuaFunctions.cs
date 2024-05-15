using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CustomLuaFunctions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      RegisterLuaFunctions();
    }

    private void OnEnable()
    {
        RegisterLuaFunctions();
    }

    private void OnDisable()
    {
        DeregisterLuaFunctions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RegisterLuaFunctions()
    {
        Lua.RegisterFunction(nameof(SurpassedTime), this, SymbolExtensions.GetMethodInfo(() => SurpassedTime(string.Empty)));
        Lua.RegisterFunction(nameof(BeforeTimeslot), this, SymbolExtensions.GetMethodInfo(() => BeforeTimeslot(string.Empty)));
        Lua.RegisterFunction(nameof(WithinTimeRange), this, SymbolExtensions.GetMethodInfo(() => WithinTimeRange(string.Empty, string.Empty)));
        Lua.RegisterFunction(nameof(WithinGracePeriod), this, SymbolExtensions.GetMethodInfo(() => WithinGracePeriod(string.Empty, 0)));
        Lua.RegisterFunction(nameof(FreezeClock), this, SymbolExtensions.GetMethodInfo(() => FreezeClock(false)));
        Lua.RegisterFunction(nameof(QuestInProgress), this, SymbolExtensions.GetMethodInfo(() => QuestInProgress(string.Empty)));
        Lua.RegisterFunction(nameof(QuestPartiallyComplete), this, SymbolExtensions.GetMethodInfo(() => QuestPartiallyComplete(string.Empty)));
        Lua.RegisterFunction(nameof(QuestInProgressButNascent), this, SymbolExtensions.GetMethodInfo(() => QuestInProgressButNascent(string.Empty)));
        Lua.RegisterFunction(nameof(LocationIDToName), this, SymbolExtensions.GetMethodInfo(() => LocationIDToName(0)));
        Lua.RegisterFunction(nameof(HourMinuteToTime), this, SymbolExtensions.GetMethodInfo(() => HourMinuteToTime(0, 0)));
      
    }

    private void DeregisterLuaFunctions()
    {
        Lua.UnregisterFunction(nameof(SurpassedTime));
        Lua.UnregisterFunction(nameof(BeforeTimeslot));
        Lua.UnregisterFunction(nameof(WithinTimeRange));
        Lua.UnregisterFunction(nameof(WithinGracePeriod));
        Lua.UnregisterFunction(nameof(FreezeClock));
        Lua.UnregisterFunction(nameof(QuestInProgress));
        Lua.UnregisterFunction(nameof(QuestPartiallyComplete));
        Lua.UnregisterFunction(nameof(QuestInProgressButNascent));
        Lua.UnregisterFunction(nameof(LocationIDToName));
        Lua.UnregisterFunction(nameof(HourMinuteToTime));
        
    }
    
    
    //lua functions
    
    public string HourMinuteToTime(double hour, double minute)
    {
        var hourString = hour.ToString();
        if (hourString.Length == 1) hourString = "0" + hourString;
        
        var minuteString = minute.ToString();
        if (minuteString.Length == 1) minuteString = "0" + minuteString;

        return hourString + ":" + minuteString;
    }
    
    public string LocationIDToName(System.Single locationID)
    {
        return DialogueManager.DatabaseManager.masterDatabase.GetLocation((int)locationID).Name;
    }
    
    public bool QuestInProgressButNascent(string quest) => QuestUtility.QuestInProgressButNascent(quest);

    public bool QuestInProgress(string quest)
    {
        return QuestUtility.QuestInProgress(quest);
    }

    public bool QuestPartiallyComplete(string quest)
    {
        return QuestUtility.QuestPartiallyComplete(quest);
    }
    
    
    
    public void FreezeClock(bool freeze)
    {
        Clock.Freeze(freeze);
    }
    
    public bool SurpassedTime(string time)
    {
       
        var timeInSeconds = Clock.ToSeconds(time);

        return Clock.CurrentTimeRaw > timeInSeconds;
    }
    
    public bool BeforeTimeslot(string time)
    {
       
        var timeInSeconds = Clock.ToSeconds(time);

        return Clock.CurrentTimeRaw < timeInSeconds;
    }
    
    public bool WithinTimeRange(string time1, string time2)
    {
        
        var time1InSeconds = Clock.ToSeconds(time1);
        var time2InSeconds = Clock.ToSeconds(time2);

        return Clock.CurrentTimeRaw > time1InSeconds && Clock.CurrentTimeRaw < time2InSeconds;
    }
    
    public bool WithinGracePeriod(string time, double gracePeriod)
    {
        
        var timeInSeconds = Clock.ToSeconds(time);
        return Clock.CurrentTimeRaw > timeInSeconds - (int)gracePeriod && Clock.CurrentTimeRaw < timeInSeconds + (int)gracePeriod;
    }
}
