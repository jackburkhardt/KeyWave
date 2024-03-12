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
        Lua.RegisterFunction(nameof(BehindTime), this, SymbolExtensions.GetMethodInfo(() => BehindTime(string.Empty)));
        Lua.RegisterFunction(nameof(WithinTimeRange), this, SymbolExtensions.GetMethodInfo(() => WithinTimeRange(string.Empty, string.Empty)));
        Lua.RegisterFunction(nameof(WithinGracePeriod), this, SymbolExtensions.GetMethodInfo(() => WithinGracePeriod(string.Empty, 0)));
    }

    private void DeregisterLuaFunctions()
    {
        Lua.UnregisterFunction(nameof(SurpassedTime));
        Lua.UnregisterFunction(nameof(BehindTime));
        Lua.UnregisterFunction(nameof(WithinTimeRange));
        Lua.UnregisterFunction(nameof(WithinGracePeriod));
    }
    
    
    //lua functions
    
    public bool SurpassedTime(string time)
    {
       
        var timeInSeconds = Clock.ToSeconds(time);

        return Clock.CurrentTimeRaw > timeInSeconds;
    }
    
    public bool BehindTime(string time)
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
