using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

public class PhoneCallPanel : UIPanel
{
    
    public float ringTime = 2.5f;
    public float totalStandbyTime = 3.5f;
    public AudioSource audioSource;
    public AudioClip ringtone;
    public AudioClip answerSound;
    public string answerAnimationTrigger;
    
    public StandardUISubtitlePanel subtitlePanel;
    public StandardUIMenuPanel menuPanel;
    public StandardUISubtitlePanel thoughtPanel;
    public DialogueActor thoughtActor;
    private bool _markForAwakeAnimation;

    private CustomDialogueUI _customDialogueUI;
    
    public UITextField contactName;
    public UITextField contactDescription;
    
    public static Action<string> OnPhoneCallStart;
    public static Action<string> OnPhoneCallEnd;

    protected override void OnEnable()
    {
        _markForAwakeAnimation = true;
        contactName.text = "";
    }
    
    public void OnValidate()
    {
        if (totalStandbyTime < ringTime)
        {
            totalStandbyTime = ringTime + 0.1f;
        }
    }

    public override void Open()
    {
        _customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        
        _customDialogueUI.OverrideDefaultPanels(subtitlePanel, subtitlePanel, menuPanel);

        if (thoughtPanel != null && thoughtActor != null)
        {
             _customDialogueUI.SetActorMenuPanelNumber( thoughtActor, MenuPanelNumber.Panel8);
            _customDialogueUI.SetActorSubtitlePanelNumber(thoughtActor, SubtitlePanelNumber.Panel2);
        }
        
        base.Open();
        if (_markForAwakeAnimation) StartCoroutine(AwakeAnimation());
        _markForAwakeAnimation = false;
       
        if (contactName.text != string.Empty) OnPhoneCallStart?.Invoke(contactName.text);
    }
    
    IEnumerator AwakeAnimation()
    {
        DialogueManager.Pause();
        
        audioSource.clip = ringtone;
        audioSource.loop = true;
        audioSource.Play();
        yield return new WaitForSeconds(ringTime);
        audioSource.Stop();
        audioSource.clip = answerSound;
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSeconds(totalStandbyTime - ringTime);
        DialogueManager.Unpause();
        GetComponent<Animator>().SetTrigger(answerAnimationTrigger);
    }

    public override void Close()
    {
        base.Close();
        OnPhoneCallEnd?.Invoke( contactName.text);
    }
    
    public void SetContactInfo( Item contact)
    {
        contactName.text = contact.Name;
        contactDescription.text = contact.Description;
    }
}
