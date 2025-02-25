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

    public StandardUISubtitlePanel thoughtPanel;
    private bool _markForAwakeAnimation;

    protected override void OnEnable()
    {
        _markForAwakeAnimation = true;
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

        if (thoughtPanel != null)
        {
            var thought = FindObjectsByType<DialogueActor>( FindObjectsInactive.Include, FindObjectsSortMode.None).First( p => p.actor == "Thought");
            FindObjectsOfType< CustomDialogueUI>().First().SetActorMenuPanelNumber( thought, MenuPanelNumber.Panel8);
            FindObjectsOfType<CustomDialogueUI>().First().SetActorSubtitlePanelNumber(thought, SubtitlePanelNumber.Panel2);
        }
        
       
        
    
        base.Open();
        if (_markForAwakeAnimation) StartCoroutine(AwakeAnimation());
        _markForAwakeAnimation = false;
       
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
}
