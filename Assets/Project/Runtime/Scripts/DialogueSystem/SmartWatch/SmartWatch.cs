using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

[CreateAssetMenu(fileName = "SmartWatch", menuName = "SmartWatch")]
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
            if (Settings.SmartWatch == null) return null;
            return Settings.SmartWatch;
        }
    }

    
    public static App GetApp(string name)
    {
        if (Settings.SmartWatch == null || Settings.SmartWatch.apps == null) return null;
        return Settings.SmartWatch.apps.Find(app => app.name == name);
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
    
    public static void GoToCurrentApp(string sequenceSuffix = "")
    {
        
        OpenApp(_currentApp ?? Settings.SmartWatch.apps[0], sequenceSuffix);
    } 
    
    
    public static void OpenApp(App app, string sequenceSuffix = "")
    {
        DialogueManager.instance.PlaySequence($"GoToConversation({app.dialogueSystemConversationTitle})" + sequenceSuffix);
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
                
        
    
