using System.Collections;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class CustomUISubtitlePanel : StandardUISubtitlePanel
{
   
    
    public string unintroducedSpeakerName;
    public UITextField conversantName;
    
    public RectTransform templateContent;
    public RectTransform accumulatedContentHolder;
    
    public bool accumulateByInstantiation;

    private bool haha;
  
    public override void Close()
    {
        base.Close(); 
        DialogueManager.instance.BroadcastMessage("OnUIPanelClose", this);
    }

    public void CloseNow()
    {
        base.Close();
    }
    
    public override void Open()
    {
        base.Open();
        DialogueManager.instance.BroadcastMessage("OnUIPanelOpen", this);
        RefreshLayoutGroups.Refresh(gameObject);
        StartCoroutine(DelayedRefresh());
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
                && !subtitle.speakerInfo.GetFieldBool("Introduced")
                    ? FormattedText.Parse(string.Format(addSpeakerNameFormat,
                        new object[] { unintroducedSpeakerName, subtitle.formattedText.text })).text
                    : FormattedText.Parse(string.Format(addSpeakerNameFormat,
                        new object[] { subtitle.speakerInfo.Name, subtitle.formattedText.text })).text;
        }
        
        if (conversantName != null) conversantName.text = subtitle.listenerInfo.Name;

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
        foreach (var go in accumulatedContentHolder.gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (go.parent == accumulatedContentHolder)
            {
                Destroy(go.gameObject);
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

    public void OnSuperceded()
    {
       CloseNow();
    }
    
    public void OnDeload()
    {
        //CloseNow();
    }
    
}
