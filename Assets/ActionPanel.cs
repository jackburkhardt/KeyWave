using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

public class ActionPanel : MonoBehaviour
{
    public bool isActionPanelActive = false;

    public static ActionPanel instance;
    
    public CircularUIMenuPanel circularUIMenuPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        
       
    }
    
    private Subtitle mostRecentSubtitle = null;
    
    private bool newConversation = false;
    

    // Start is called before the first frame update
    
    public void OnLinkedConversationStart(Transform actor)
    {
        newConversation = true;
        
    }
    public void OnConversationLine(Subtitle subtitle)
    {
        Debug.Log("Conversation line");
        if (newConversation)
        {
            newConversation = false;
            var conversation = subtitle.dialogueEntry.GetConversation().Title;
            if (!conversation.Contains("Map") && !conversation.EndsWith("Action/Base") && !conversation.EndsWith("Talk/Base") && !conversation.EndsWith("Walk/Base")) 
            {
                HidePanel();
            }
        }
        
        
        if (Field.FieldExists(subtitle.dialogueEntry.fields, "Track") && Field.LookupBool(subtitle.dialogueEntry.fields, "Track").Equals(true))
        {
       //     if (subtitle.dialogueEntry.)
        }
        
        mostRecentSubtitle = subtitle;
    }
    
    private ActionPanelButton GetButtonOfType(ActionPanelButton.ActionPanelButtonType type)
    {
        var buttons = transform.GetChildren<ActionPanelButton>();
        return buttons.FirstOrDefault(button => button.type == type);
    }
    
    private ActionPanelButton GetButtonOfType(string s) => GetButtonOfType((ActionPanelButton.ActionPanelButtonType) Enum.Parse(typeof(ActionPanelButton.ActionPanelButtonType), s));
    
    public void OnConversationEnd()
    {
        Debug.Log("Conversation end");
        var currentConversation = mostRecentSubtitle.dialogueEntry.GetConversation().Title;

        var conversationType = currentConversation.Split("/").Length > 3 ? currentConversation.Split("/")[^2] : string.Empty;

        if (conversationType is "Action" or "Talk" or "Walk")
        {
            Debug.Log("Showing panel " + conversationType);
            ShowPanel(GetButtonOfType(conversationType));
        }
        
        if (currentConversation.EndsWith("Base"))
        {
            Debug.Log("Showing panel");
            ShowPanel();
        }
        
        //else HidePanel();
    }

    private void OnLoad()
    {
        HidePanel();
    }
    
    public void OnConversationStart()
    {
        Debug.Log("Conversation start");
        newConversation = true;
        EvaluatePanel();
    }

    public void EvaluatePanel()
    {
        if (Location.PlayerLocation == null) return;
        var location = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(location, "Current Sublocation").asString;
        
        if (!string.IsNullOrEmpty(sublocation)) location += "/" + sublocation;

        var actionConversationExists = DialogueManager.masterDatabase.GetConversation($"{location}/Action/Base") != null;
        var talkConversationExists = DialogueManager.masterDatabase.GetConversation($"{location}/Talk/Base") != null;
    //    Debug.Log(DialogueManager.masterDatabase.GetConversation($"{Location.PlayerLocation.name}/Walk/Base"));
        var walkConversationExists = DialogueManager.masterDatabase.GetConversation($"{Location.PlayerLocation.name}/Walk/Base") != null;
      //  Debug.Log(walkConversationExists);

        var buttons = transform.GetChildren<ActionPanelButton>();
        
        foreach (var button in buttons)
        {
            switch (button.type)
            {
                case ActionPanelButton.ActionPanelButtonType.Walk:
                    button.gameObject.SetActive(walkConversationExists);
                    break;
                case ActionPanelButton.ActionPanelButtonType.Talk:
                    button.gameObject.SetActive(talkConversationExists);
                    break;
                case ActionPanelButton.ActionPanelButtonType.Action:
                    button.gameObject.SetActive(actionConversationExists);
                    break;
            }
        }
    }
    
    public void ShowPanel(string type)
    {
        if (type == string.Empty) ShowPanel();
        else ShowPanel(GetButtonOfType(type));
    }

    public void ShowPanel(ActionPanelButton button = null)
    {
        if (!isActionPanelActive && !isDummyPanelActive)
        {
            GetComponent<Animator>().SetTrigger("Show");
        }
        
        isActionPanelActive = true;
        isDummyPanelActive = false;
        
        EvaluatePanel();

        if (button == null)
        {
            button = GetComponentsInChildren<ActionPanelButton>()[0];
        }
        
        
        StartCoroutine(SimulateClickAfterDelay(button, 0.5f));
    }
    
    private bool isDummyPanelActive = false;

    public void ShowDummyPanel()
    {
        if (!isActionPanelActive)
        {
            GetComponent<Animator>().SetTrigger("Show");
        }
        
        isDummyPanelActive = true;
        
        EvaluatePanel();
        var buttons = instance.transform.GetChildren<ActionPanelButton>();
        Debug.Log(GameManager.gameState.PlayerLocation);
        Debug.Log(DialogueManager.masterDatabase.GetConversation("Hotel/Action/Base"));
        
        foreach (var button in buttons)
        {
            button.label.gameObject.SetActive(false);
            button.DarkenButton(false);
            button.GetComponent<Button>().interactable = false;
        }
    }
    
    private IEnumerator SimulateClickAfterDelay(ActionPanelButton button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.OnClick();
    }

    public static void ShowLabelAndHideAllOthers(ActionPanelButton panelButton)
    {
        var buttons = instance.transform.GetChildren<ActionPanelButton>();
        
        foreach (var button in buttons)
        {
            button.GetComponent<Button>().interactable = true;
            button.label.gameObject.SetActive(button == panelButton);
            button.DarkenButton(button != panelButton);
        }
    }

    public void HidePanel()
    {
        if (!isActionPanelActive) return;
        isActionPanelActive = false;
        GetComponent<Animator>().SetTrigger("Hide");
    }

}

public class SequencerCommandSetActionPanel : SequencerCommand
{
    private void Awake()
    {
        var value = GetParameterAsBool(0);
        var type = GetParameter(1, "");
        
        if (value)
        {
            if (type == "dummy")
            {
                ActionPanel.instance.ShowDummyPanel();
            }
            else
            {
                ActionPanel.instance.ShowPanel(type);
            }
        }
        else
        {
            ActionPanel.instance.HidePanel();
        }
    }
}
