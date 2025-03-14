using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class SmartWatchPanel : UIPanel
{
    
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;

    private static SmartWatchAppPanel _currentApp;
    public static Action<SmartWatchAppPanel> onAppOpen;
    
    [ConversationPopup] public string conversation;

    public HomeButtonPanel homeButton;
    
    public List<SmartWatchAppPanel> appPanels =>
        GetComponentsInChildren<SmartWatchAppPanel>(true).ToList();

    public void Awake()
    {
        foreach (var appPanel in appPanels)
        {
            appPanel.gameObject.SetActive(false);
        }
    }

    public override void Open()
    {
        if (!DialogueManager.isConversationActive) DialogueManager.StartConversation(conversation);
        if (_currentApp == null)
        {
            var defaultApp = GetAllApps().Find(x => x.LookupBool("Is Default"));
            _currentApp = appPanels.Find(p => p.Name == defaultApp.Name);
        }
        OpenApp( _currentApp.Name);
        base.Open();
    }
    
    public void OpenApp( string appName)
    {
        var appPanel = appPanels.FirstOrDefault(p => p.Name == appName);
        
        foreach (var smartWatchApp in FindObjectsByType< SmartWatchAppPanel>(  FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (smartWatchApp != appPanel)
            {
                smartWatchApp.panel.Close();
            }
        }
        
        appPanel.panel.Open();
      
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        onAppOpen += OnAppOpen;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        onAppOpen -= OnAppOpen;
    }
    

    public void OnConversationStart()
    {

        if (_currentApp == null) return;
        switch (_currentApp.Name)
        {
            case "Phone":
            {
                GetComponent<Animator>().SetTrigger("Down");
                break;
            }

            case "Home":
            {
                break;
            }

            default:
                
                GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
                break;
        }
    }

    public static List<Item> GetAllApps()
    {
        return GameManager.settings.dialogueDatabase.items.FindAll(p => p.IsApp);
    }

    public void ForceDefaultApp()
    {
        _currentApp = null;
        DialogueManager.instance.GoToConversation( conversation);
        Open();
    }

    public void ResetCurrentApp()
    {
        _currentApp = null;
    }

    public void OnLinkedConversationStart()
    {
        OnConversationStart();
    }

    private void OnAppOpen(SmartWatchAppPanel app)
    {
        GetComponent< Animator>().SetTrigger( focusAnimationTrigger);
        GetComponent< Animator>().SetTrigger( showAnimationTrigger);
        GetComponent<Animator>().SetTrigger( app.Name);
        FindObjectOfType<CustomDialogueUI>().ClearAllDefaultOverrides();
    }
    
    public void OnGameSceneStart()
    {
        _currentApp = null;
        GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
    }

    public void OnGameSceneEnd()
    {
        _currentApp = null;
        GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
    }
    
}
