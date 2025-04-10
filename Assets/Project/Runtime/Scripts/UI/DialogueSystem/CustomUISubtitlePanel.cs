using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CustomUISubtitlePanel : StandardUISubtitlePanel
{
   
    
    public UITextField conversantName;

    public UITextField dialogueEntryTitle;
    
    public RectTransform templateContent;
    public RectTransform accumulatedContentHolder;
    public bool accumulateByInstantiation;
    
    [Tooltip( "When making decisions, the Dialogue System will force this menu panel when this subtitle panel is open, regardless of any DialogueActor settings.")]
    public StandardUIMenuPanel forceOverrideMenuPanel;
    
    InputAction clickAction;
    InputAction submitAction;
    
    private InputSystemUIInputModule _inputSystemUIInputModule;
    private EventSystem _eventSystem;
    private CustomDialogueUI _customDialogueUI;

    public static CustomUISubtitlePanel latestInstance;


    protected void OnValidate()
    {
        _customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
    }


    protected override void Awake()
    {
        base.Awake();
        var typewriter = subtitleText.gameObject.GetComponentInChildren<TextMeshProTypewriterEffect>(true);
        if (typewriter != null)
        {
            typewriter.onBegin.AddListener( Show);
            typewriter.onFirstCharacter.AddListener( Show);
            typewriter.onEnd.AddListener( Show);
        }
        
       
        _customDialogueUI = FindObjectOfType<CustomDialogueUI>();
    }

    public void OnGameSceneStart()
    {
        ClearContents();
    }
    
    public void OnGameSceneEnd()
    {
        Close();
        ClearContents();
    }
    
    
    private void Show()
    {
        if (!string.IsNullOrEmpty(showAnimationTrigger)) GetComponent<Animator>().SetTrigger(showAnimationTrigger);
        if (!string.IsNullOrEmpty( focusAnimationTrigger)) GetComponent<Animator>().SetTrigger( focusAnimationTrigger);
    }

    
    public override void Open()
    {
        Show();
        
        latestInstance = this;
        
        // sometimes the Dialogue System will use the wrong menu panels, so this is a workaround to force the correct one
        if (forceOverrideMenuPanel != null)
        {
            _customDialogueUI.ForceOverrideMenuPanel( forceOverrideMenuPanel);
        }
      
        if (TryGetComponent<Button>( out var button))
        {
            button.enabled = true;
        }
        
        base.Open();
        RefreshLayoutGroups.Refresh(gameObject);
        StartCoroutine(DelayedRefresh());
        
        
    }
    
    public override void Close()
    {
        if (forceOverrideMenuPanel != null && _customDialogueUI != null) _customDialogueUI.ForceOverrideMenuPanel( null);
        
        if (latestInstance == this)
        {
            latestInstance = null;
        }
        
        if (TryGetComponent<Button>( out var button))
        {
            button.enabled = false;
        }
        base.Close(); 
    }

    protected override void OnHidden()
    {
        base.OnHidden();
        ClearContents();
    }
    
    private IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(0.1f);
        RefreshLayoutGroups.Refresh(gameObject);
    }

    
    
    public override void ShowSubtitle(Subtitle subtitle)
    {
        
        if (accumulateText && accumulateByInstantiation) RevealAccumulatedContent();
        
        base.ShowSubtitle(subtitle);
        
        if (accumulateText && accumulateByInstantiation) AccumulateContentSecretly(subtitle);
      
    }


    protected override void SetSubtitleTextContent(Subtitle subtitle)
    {
        if (addSpeakerName && !string.IsNullOrEmpty(subtitle.speakerInfo.Name))
        {
            subtitle.formattedText.text = 
                Field.FieldExists(
                    DialogueManager.instance.masterDatabase.GetActor(subtitle.speakerInfo.nameInDatabase).fields,
                    "Introduced")
                && !DialogueLua.GetActorField(subtitle.speakerInfo.Name, "Introduced" ).asBool
                    ? subtitle.formattedText.text
                    : FormattedText.Parse(string.Format(addSpeakerNameFormat,
                        new object[] { subtitle.speakerInfo.Name, subtitle.formattedText.text })).text;
        }
        
        if (conversantName != null) conversantName.text = subtitle.listenerInfo.Name;
        
        if (dialogueEntryTitle != null)
        {
            dialogueEntryTitle.text = subtitle.dialogueEntry.Title;
        }

        TypewriterUtility.StopTyping(subtitleText);
        var previousText = accumulateText  && !accumulateByInstantiation ? accumulatedText : string.Empty;
        if (accumulateText && !string.IsNullOrEmpty(subtitle.formattedText.text))
        {
            if (numAccumulatedLines < maxLines)
            {
                numAccumulatedLines += (1 + NumCharOccurrences('\n', subtitle.formattedText.text));
            }
            else
            {
                // If we're at the max number of lines, remove the first line from the accumulated text:
                previousText = RemoveFirstLine(previousText);
            }
        }

        var previousChars = accumulateText
            ? UITools.StripRPGMakerCodes(Tools.StripTextMeshProTags(Tools.StripRichTextCodes(previousText))).Length
            : 0;
        SetFormattedText(subtitleText, previousText, subtitle);
        if (accumulateText) accumulatedText = UITools.StripRPGMakerCodes(subtitleText.text) + "\n";
        if (scrollbarEnabler != null && !HasTypewriter())
        {
            scrollbarEnabler.CheckScrollbarWithResetValue(0);
        }
        else if (delayTypewriterUntilOpen && !hasFocus)
        {
            DialogueManager.instance.StartCoroutine(StartTypingWhenFocused(subtitleText, subtitleText.text,
                previousChars));
        }
        else
        {
            TypewriterUtility.StartTyping(subtitleText, subtitleText.text, previousChars);
        }
    }
    
    
    public void ClearContents()
    {
        accumulatedText = string.Empty;
        numAccumulatedLines = 0;
        if (accumulatedContentHolder != null)
        {
            foreach (var go in accumulatedContentHolder.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (go.parent == accumulatedContentHolder)
                {
                    Destroy(go.gameObject);
                }
            }
        }
        subtitleText.text = string.Empty;
    }

    private void RevealAccumulatedContent()
    {
        foreach (var go in accumulatedContentHolder.gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (go.parent == accumulatedContentHolder)
            {
                go.gameObject.SetActive(true);
            }
        }
    }

    private void AccumulateContentSecretly(Subtitle subtitle)
    {
        
        StartCoroutine(Accumulate(subtitle));
        
        IEnumerator Accumulate(Subtitle sub)
        {
       
          
            var typewriter = subtitleText.gameObject.GetComponentInChildren<AbstractTypewriterEffect>();
            
            while (!typewriter.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            
            if (!string.IsNullOrWhiteSpace(sub.formattedText.text))
            {
                var duplicate = Instantiate(templateContent, accumulatedContentHolder);
                var duplicateTypewriter = duplicate.GetComponentInChildren<AbstractTypewriterEffect>();
        
                if (duplicateTypewriter != null)
                {
                    duplicateTypewriter.Stop();
                    duplicateTypewriter.enabled = false;
                }
            
                duplicate.gameObject.SetActive(false);
            }
        
            RefreshLayoutGroups.Refresh(gameObject);
        }
    }
    
    
}
