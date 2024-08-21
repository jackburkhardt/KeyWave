using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CustomUISubtitlePanel : StandardUISubtitlePanel
{
    public bool hideOnConversationEnd;

  
    public override void Close()
    {
        if (!hideOnConversationEnd) return;
        base.Close();
    }

    public void CloseNow()
    {
        base.Close();
    }

    public override void HideSubtitle(Subtitle subtitle)
    {
        if (panelState != PanelState.Closed) Unfocus();
        CloseNow();
    }
    
    public override void ShowSubtitle(Subtitle subtitle)
    {
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



    public void OnSuperceded()
    {
       CloseNow();
    }
    
    public void OnDeload()
    {
        CloseNow();
    }
    
}
