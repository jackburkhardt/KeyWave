using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;

public class CustomDialogueUI : StandardDialogueUI
{

    [Tooltip("Dialogue Manager's Include Sim Status must be ticked. This checkbox omits Sim Status from saved games to keep them smaller.")]
    public bool includeSimStatusInSavedGames = false;

    public string showInvalidFieldName = "Show Invalid";

    [HideIf("_showInvalidAlways")]
    [SerializeField]
    private bool _showInvalidByDefault;
    
    public bool ShowInvalidByDefault
    {
        get => _showInvalidByDefault && !_showInvalidAlways;
        set => _showInvalidByDefault = value;
    }
    
    [HideIf("_showInvalidByDefault")]
    [SerializeField]
    private bool _showInvalidAlways = false;


    public bool ShowInvalidAlways
    {
        get => _showInvalidAlways && !_showInvalidByDefault;
        set => _showInvalidAlways = value;
    }
    
   // private new CustomUIDialogueControls conversationUIElements;

    public override void Awake()
    {
        conversationUIElements = new CustomUIDialogueControls(conversationUIElements);
        base.Awake();
    }
    
    private void VerifyAssignments()
    {
        if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        if (DialogueDebug.logWarnings && verifyPanelAssignments)
        {
            if (alertUIElements.alertText.gameObject == null) Debug.LogWarning("Dialogue System: No UI text element is assigned to Standard Dialogue UI's Alert UI Elements.", this);
            if (conversationUIElements.subtitlePanels.Length == 0) Debug.LogWarning("Dialogue System: No subtitle panels are assigned to Standard Dialogue UI.", this);
            if (conversationUIElements.menuPanels.Length == 0) Debug.LogWarning("Dialogue System: No response menu panels are assigned to Standard Dialogue UI.", this);
        }
    }


    #region Custom

    public override void Start()
    {
        PersistentDataManager.includeSimStatus = includeSimStatusInSavedGames;
        base.Start();
        
    }

    private StandardUIMenuPanel _currentMenuOverride;
    
    public override void ForceOverrideMenuPanel(StandardUIMenuPanel customPanel)
    {
        conversationUIElements.standardMenuControls.ForceOverrideMenuPanel(customPanel);
    }

    
    public void ClearForcedMenuOverride(StandardUIMenuPanel customPanel)
    {
        if (customPanel == _currentMenuOverride) conversationUIElements.standardMenuControls.ForceOverrideMenuPanel( null);
    }
    
    public void OverrideDefaultPanels( StandardUISubtitlePanel pcSubtitlePanel = null, StandardUISubtitlePanel npcSubtitlePanel = null, StandardUIMenuPanel menuPanel = null)
    {
        if (pcSubtitlePanel != null) conversationUIElements.standardSubtitleControls.defaultPCPanel = pcSubtitlePanel;
        if (npcSubtitlePanel != null) conversationUIElements.standardSubtitleControls.defaultNPCPanel = npcSubtitlePanel;
        if (menuPanel != null) conversationUIElements.standardMenuControls.defaultPanel = menuPanel;
    }
    
    public void ClearAllDefaultOverrides()
    {
        conversationUIElements.standardSubtitleControls.defaultPCPanel = conversationUIElements.defaultPCSubtitlePanel;
        conversationUIElements.standardSubtitleControls.defaultNPCPanel = conversationUIElements.defaultNPCSubtitlePanel;
        conversationUIElements.standardMenuControls.defaultPanel = conversationUIElements.defaultMenuPanel;
    }
    

    public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
        DialogueManager.conversationModel.GetConversationOverrideSettings( DialogueManager.currentConversationState).skipPCSubtitleAfterResponseMenu = true;
        //generate new responses
        
        responses = CheckInvalidResponses(responses);
        base.ShowResponses(subtitle, responses, timeout);
        
    }

    public Response[] CheckInvalidResponses(Response[] responses)
    {
        if (!HasAnyInvalid(responses)) return responses;
        var list = new List<Response>();
        for (int i = 0; i < responses.Length; i++)
        {
            var response = responses[i];
            //--- Was: if (response.enabled || Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName))
            //--- To allow runtime changes, we need to use a more sophisticated AllowShowInvalid() method:
            if (response.enabled || AllowShowInvalid(response) || ShowInvalidAlways)
            {
                list.Add(response);
            }
        }
        return list.ToArray();
    }

    private bool HasAnyInvalid(Response[] responses)
    {
       // Debug.Log("Checking for invalid responses");
        if (responses == null)
        {
          //  Debug.Log("No responses");
            return false;
        }
        for (int i = 0; i < responses.Length; i++)
        {
    
            if (!responses[i].enabled)
            {
            //    Debug.Log("Found invalid response: " + responses[i].formattedText.text);
                return true;
            }
        }
        
        
//        Debug.Log("No invalid responses");
      //  Debug.Log(DialogueManager.displaySettings.inputSettings.includeInvalidEntries);
        return false;
    }

    private bool AllowShowInvalid(Response response)
    {
        
        return Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName);
    }

    public override void ShowSubtitle(Subtitle subtitle)
    {
        //subtitle.formattedText.text = Regex.Replace(subtitle.formattedText.text, @"[^\x20-\x7F]", "");
        base.ShowSubtitle(subtitle);
        
        
    }

    public void OnConversationLine(Subtitle subtitle)
    {
        //overkill but I'm desperate
        DialogueManager.conversationModel.GetConversationOverrideSettings( DialogueManager.currentConversationState).skipPCSubtitleAfterResponseMenu = true;
        
        
        if (Field.FieldExists(subtitle.dialogueEntry.fields, "Randomize Next Entry") && Field.LookupBool(subtitle.dialogueEntry.fields, "Randomize Next Entry"))
        {
            Debug.Log("Randomizing next entry");
            DialogueManager.instance.conversationController.randomizeNextEntry = true;
            DialogueManager.instance.conversationController.randomizeNextEntryNoDuplicate = true;
        }
        
    }
    


    private void OnConversationStart()
    {
        DialogueManager.conversationModel.GetConversationOverrideSettings( DialogueManager.currentConversationState).skipPCSubtitleAfterResponseMenu = true;
    }

    private void OnLinkedConversationStart()
    {
        DialogueManager.conversationModel.GetConversationOverrideSettings( DialogueManager.currentConversationState).skipPCSubtitleAfterResponseMenu = true;
    }

    #endregion
}

public class CustomUIDialogueControls : StandardUIDialogueControls
{
    StandardUIDialogueControls _standardUIDialogueControls;

    public new CustomUISubtitleControls standardSubtitleControls;
    
    public CustomUIDialogueControls(StandardUIDialogueControls standardUIDialogueControls) 
    {
        _standardUIDialogueControls = standardUIDialogueControls;
        
        mainPanel = standardUIDialogueControls.mainPanel;
        dontDeactivateMainPanel = standardUIDialogueControls.dontDeactivateMainPanel;
        waitForMainPanelOpen = standardUIDialogueControls.waitForMainPanelOpen;
        subtitlePanels = standardUIDialogueControls.subtitlePanels;
        defaultNPCSubtitlePanel = standardUIDialogueControls.defaultNPCSubtitlePanel;
        defaultPCSubtitlePanel = standardUIDialogueControls.defaultPCSubtitlePanel;
        allowOpenSubtitlePanelsOnStartConversation = standardUIDialogueControls.allowOpenSubtitlePanelsOnStartConversation;
        allowDialogueActorCustomPanels = standardUIDialogueControls.allowDialogueActorCustomPanels;
        menuPanels = standardUIDialogueControls.menuPanels;
        defaultMenuPanel = standardUIDialogueControls.defaultMenuPanel;
        useFirstResponseForMenuPortrait = standardUIDialogueControls.useFirstResponseForMenuPortrait;
        waitForClose = standardUIDialogueControls.waitForClose;
        
        standardSubtitleControls = new CustomUISubtitleControls(_standardUIDialogueControls.standardSubtitleControls);
        typeof(StandardUIDialogueControls).GetField("m_standardSubtitleControls", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, standardSubtitleControls);
    }
}                   

public class CustomUISubtitleControls : StandardUISubtitleControls
{
    StandardUISubtitleControls _standardUISubtitleControls;
    
    public CustomUISubtitleControls(StandardUISubtitleControls standardUISubtitleControls) 
    {
        _standardUISubtitleControls = standardUISubtitleControls;
    }

    protected override void SupercedeOtherPanelsInList(List<StandardUISubtitlePanel> list, StandardUISubtitlePanel newPanel)
    {
       
        for (int i = 0; i < list.Count; i++)
        {
            var panel = list[i];
            if (panel == null || panel == newPanel) continue;
            if (panel.isOpen)
            {
                if (UITools.CanBeSuperceded(panel.visibility))
                {
                    panel.Close();
                    panel.SendMessage("OnSuperceded", SendMessageOptions.DontRequireReceiver);
                   
                }
                else
                {
                    panel.Unfocus();
                }
            }
        }
    }
    
    
}
