using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CustomUISubtitlePanel : StandardUISubtitlePanel
{
    public bool hideOnConversationEnd;

    public bool useAlternateCloseMethod;
    public string unintroducedSpeakerName;
  
    public override void Close()
    {
        if (!useAlternateCloseMethod)
        {
            base.Close(); return;
        }
        
        if (!hideOnConversationEnd) return;
        base.Close();
    }

    public void CloseNow()
    {
        base.Close();
    }

    public override void HideSubtitle(Subtitle subtitle)
    {
        if (!useAlternateCloseMethod)
        {
            base. HideSubtitle(subtitle); return;
        }
        
        if (panelState != PanelState.Closed) Unfocus();
        CloseNow();
    }
    
    public override void ShowSubtitle(Subtitle subtitle)
    {
        if (!useAlternateCloseMethod)
        {
            base.ShowSubtitle(subtitle); return;
        }
        
        var supercedeOnActorChange = waitForClose && isOpen && visibility == UIVisibility.UntilSupercededOrActorChange &&
                                     subtitle != null && lastActorID != subtitle.speakerInfo.id;
        if ((waitForClose && dialogueUI.AreAnyPanelsClosing(this)) || supercedeOnActorChange)
        {
            if (supercedeOnActorChange) CloseNow();
            StopShowAfterClosingCoroutine();
            m_showAfterClosingCoroutine = DialogueManager.instance.StartCoroutine(ShowSubtitleAfterClosing(subtitle));
        }
        else
        {
            ShowSubtitleNow(subtitle);
        }
    }


    protected override void SetSubtitleTextContent(Subtitle subtitle)
    {

        if (addSpeakerName && !string.IsNullOrEmpty(subtitle.speakerInfo.Name))
        {
            subtitle.formattedText.text =
                Field.FieldExists(
                    DialogueManager.instance.masterDatabase.GetActor(subtitle.speakerInfo.nameInDatabase).fields,
                    "Introduced")
                && !subtitle.speakerInfo.GetFieldBool("Introduced")
                    ? FormattedText.Parse(string.Format(addSpeakerNameFormat,
                        new object[] { unintroducedSpeakerName, subtitle.formattedText.text })).text
                    : FormattedText.Parse(string.Format(addSpeakerNameFormat,
                        new object[] { subtitle.speakerInfo.Name, subtitle.formattedText.text })).text;
        }



        TypewriterUtility.StopTyping(subtitleText);
        var previousText = accumulateText ? accumulatedText : string.Empty;
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


    public void OnSuperceded()
    {
       CloseNow();
    }
    
    public void OnDeload()
    {
        CloseNow();
    }
    
}
