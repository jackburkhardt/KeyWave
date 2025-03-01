using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

[CreateAssetMenu(fileName = "SmartWatch", menuName = "SmartWatch/SmartWatch Settings")]
public class SmartWatch : ScriptableObject
{

    private static App _currentApp;

    [Serializable]
    public class App
    {
        public string name;
        public string dialogueSystemConversationTitle;
        [ReadOnly] public string animatorTrigger;
    }

    public List<App> apps;
    
    [HideInInspector] public List<string> appNames => apps.ConvertAll(app => app.name);

   
    public static SmartWatch instance {
        get
        {
            return GameManager.settings.SmartWatch;
        }
    }

    
    public static App GetApp(string name)
    {
        if (GameManager.settings.SmartWatch == null || GameManager.settings.SmartWatch.apps == null) return null;
        
        if (name == "Default")
        {
            return GameManager.settings.SmartWatch.apps[0];
        }
        
        if (name == "Current")
        {
            return _currentApp ?? GameManager.settings.SmartWatch.apps[0];;
        }
        
        return GameManager.settings.SmartWatch.apps.Find(app => app.name == name);
    }
    
    public static List<App> GetAllApps()
    {
        return GameManager.settings.SmartWatch.apps;
    }
    
    public static Action<App> OnAppOpen;
    
    
    private void OnEnable()
    {
        OnAppOpen += SetCurrentApp;
        
    }
    
    private void OnDisable()
    {
        OnAppOpen -= SetCurrentApp;
    }
    
    private static void SetCurrentApp(App app)
    {
        _currentApp = app;
    }
    
    public static void ResetCurrentApp()
    {
        _currentApp = null;
    }
    
    public static void GoToCurrentApp()
    {
        OpenApp(_currentApp ?? GameManager.settings.SmartWatch.apps[0]);
    } 
    
    public static void GoToDefaultApp()
    {
        OpenApp(GameManager.settings.SmartWatch.apps[0]);
    }
    
    public static App GetCurrentApp()
    {
        return _currentApp ?? GameManager.settings.SmartWatch.apps[0];
    }
    
    //SetQuestState("Hotel/Action/Breakfast", "success")
    public static void OpenApp(App app)
    {
        DialogueManager.instance.GoToConversation(app.dialogueSystemConversationTitle, true);
    }

    private void OnValidate()
    {
        foreach (var app in apps)
        {
            if (string.IsNullOrEmpty(app.animatorTrigger))
            {
                app.animatorTrigger = app.name;
            }
        }
    }
    
}

public class SequencerCommandOpenApp : SequencerCommand
{
    public void Awake()
    {
        var appName = GetParameter(0);
        if (string.IsNullOrEmpty(appName)) return;
        
        var app = SmartWatch.GetApp(appName);
        if (app == null)
        {
            Debug.Log($"App {appName} not found in SmartWatch.");
            return;
        }
        SmartWatch.OpenApp(app);
    }
}
                
        
    
