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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The SmartWatchPanel is the main panel that contains all the smartwatch apps. It is best thought of as an enclosure for the apps themselves.
// Opening the SmartWatchPanel will open _currentApp, which is probably the Actions app by default.
// Usually, the SmartWatchPanel will be opened by a Dialogue Entry's script during a conversation, using the function SetSmartWatch.
// Buttons are populated when the ResponseMenu calls ShowResponses(). This is probably done using a Dialogue Entry node with an [f] tag. 

// In summary: If the SmartWatchPanel never opens, make sure that Open() is being called, or SetSmartWatch(true) is called in the Dialogue System.
// If apps are not populating with buttons, make sure that the conversation forces a ResponseMenu with the [f] tag.

public class SmartWatchPanel : UIPanel
{
    
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;

    private static SmartWatchAppPanel _currentApp;
    public static Action<SmartWatchAppPanel> onAppOpen;
    
    // The conversation that will be started when the SmartWatchPanel is opened, if this conversation is not already active.
    // This conversation must end with a dialogue entry with an [f] tag, which will trigger the SmartWatchApp's ShowResponsesNow method.
    [ConversationPopup] public string conversation;

    public HomeButtonPanel homeButton;
    
    public List<SmartWatchAppPanel> appPanels =>
        GetComponentsInChildren<SmartWatchAppPanel>(true).ToList();

    [Tooltip( "Most smartwatch apps are menu panels in disguise, but the Dialogue System sometimes uses the wrong menu panel. This will force the correct one.")]
    public bool forceOverrideMenuPanel;

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
        
        // sometimes the Dialogue System will use the wrong menu panels, so this is a workaround to force the correct one
        if (forceOverrideMenuPanel)
        {
            var menuPanel = _currentApp?.GetComponentInChildren<StandardUIMenuPanel>();
            
            if (menuPanel != null) // not all apps are menu panels
            {
                FindObjectOfType<CustomDialogueUI>().ForceOverrideMenuPanel( menuPanel);
            }
        }
        
        
        var appPanel = appPanels.FirstOrDefault(p => p.Name == appName);
        
        foreach (var smartWatchApp in FindObjectsByType< SmartWatchAppPanel>(  FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (smartWatchApp != appPanel)
            {
                smartWatchApp.panel.Close();
            }
        }
        
        appPanel.panel.Open();
        _currentApp = appPanel;
      
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
                // Moves the whole SmartWatch down during a phone call.
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

    public void OpenRootApp()
    {
        var rootApp = GetAllApps().Find(x => x.LookupBool("Is Root"));
        _currentApp = appPanels.Find(p => p.Name == rootApp.Name);
        
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

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(SceneManager.sceneCount);
            Debug.Log(SceneManager.GetActiveScene().name);
        }
    }
}
