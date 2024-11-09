using System;
using System.Collections;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    public bool isActionPanelActive = false;

    public static ActionPanel instance;

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
        var buttons = transform.GetComponentsInChildren<ActionPanelButton>();
        return buttons.FirstOrDefault(button => button.type == type);
    }
    
    private ActionPanelButton GetButtonOfType(string s) => GetButtonOfType((ActionPanelButton.ActionPanelButtonType) Enum.Parse(typeof(ActionPanelButton.ActionPanelButtonType), s));
    

    private void OnLoad()
    {
        HidePanel();
    }
    
    public void OnConversationStart()
    {
        newConversation = true;
        EvaluatePanel();
    }

   

    public void EvaluatePanel()
    {
        EvaluatePanel(out var _);
    }

    public void EvaluatePanel(out ActionPanelButton fallBackButton, ActionPanelButton tryButton = null)
    {
        fallBackButton = null;
        var buttons = transform.GetComponentsInChildren<ActionPanelButton>(true);
        foreach (var button in buttons)  button.gameObject.SetActive(button.ConversationCheck());
        fallBackButton = tryButton != null ? buttons.FirstOrDefault(p => p.type == tryButton.type && p.ConversationCheck()) : null;
        fallBackButton ??= GetComponentsInChildren<ActionPanelButton>()[0];
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
        
        EvaluatePanel(out button, button);

        Debug.Log("Showing panel with button " + button.type);
        
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
        var buttons = instance.transform.GetComponentsInChildren<ActionPanelButton>();
        
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
        var buttons = instance.transform.GetComponentsInChildren<ActionPanelButton>();
        
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

        return;
        
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
