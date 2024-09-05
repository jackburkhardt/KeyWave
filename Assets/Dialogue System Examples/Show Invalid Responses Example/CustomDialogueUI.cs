using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;

public class CustomDialogueUI : StandardDialogueUI
{

    [Tooltip("Dialogue Manager's Include Sim Status must be ticked. This checkbox omits Sim Status from saved games to keep them smaller.")]
    public bool includeSimStatusInSavedGames = false;

    public string showInvalidFieldName = "Show Invalid";

    public bool showInvalidByDefault;
    
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

    public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
        responses = CheckInvalidResponses(responses);
        base.ShowResponses(subtitle, responses, timeout);
    }

    private Response[] CheckInvalidResponses(Response[] responses)
    {
        if (!HasAnyInvalid(responses)) return responses;
        var list = new List<Response>();
        for (int i = 0; i < responses.Length; i++)
        {
            var response = responses[i];
            //--- Was: if (response.enabled || Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName))
            //--- To allow runtime changes, we need to use a more sophisticated AllowShowInvalid() method:
            if (response.enabled || AllowShowInvalid(response))
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
        //    Debug.Log("Checking response: " + responses[i].formattedText.text);
         //   Debug.Log("Response condition: " + responses[i].destinationEntry.conditionsString);
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
        // See if this response has a "Show Invalid" field in Lua:
        var luaResult = Lua.Run("return Dialog[" + response.destinationEntry.id + "].Show_Invalid");
        if (luaResult.Equals(Lua.noResult) || luaResult.asString == "nil")
        {
            // If not, return the design-time Show Invalid field value from the database:
            if (Field.FieldExists(response.destinationEntry.fields, showInvalidFieldName))
            {
                return Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName);
            }
            else
            {
                // Not sure how to get this
               return showInvalidByDefault;
            }
        }
        else
        {
            // Otherwise return the runtime Lua value:
            return luaResult.asBool;
        }
    }

    public override void ShowSubtitle(Subtitle subtitle)
    {
        subtitle.formattedText.text = Regex.Replace(subtitle.formattedText.text, @"[^\x20-\x7F]", "");
        base.ShowSubtitle(subtitle);
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
