using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;

[CreateAssetMenu(fileName = "SmartWatch", menuName = "SmartWatch")]
public class SmartWatch : ScriptableObject
{

    [ReadOnly] public bool isInstance;
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

    private void OnValidate()
    {
        if (_instance == null)
            AddressableLoader.RequestLoad<SmartWatch>("SmartWatch.asset", watch =>
            {
                _instance = watch;
                if (watch != null) OnValidate();
            });


        foreach (var app in apps)
        {
            app.animatorTrigger = app.name;
        }

        isInstance = _instance == this;

    }

    private static SmartWatch _instance;

    public static SmartWatch instance
    {
        get
        {
            if (_instance == null)
            {
                AddressableLoader.RequestLoad<SmartWatch>("SmartWatch.asset", watch =>
                {
                    _instance = watch;
                });
            }
            return _instance;
        }
    }
    
    public static App GetApp(string name)
    {
        return _instance.apps.Find(app => app.name == name);
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
    
    public static void GoToCurrentApp(string sequenceSuffix = "")
    {
        OpenApp(_currentApp ?? instance.apps[0], sequenceSuffix);
    } 
    
    
    public static void OpenApp(App app, string sequenceSuffix = "")
    {
        DialogueManager.instance.PlaySequence($"GoToConversation({app.dialogueSystemConversationTitle})" + sequenceSuffix);
    }
    
    
    
}
                
        
    
