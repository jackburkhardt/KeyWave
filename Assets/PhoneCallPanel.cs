using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class PhoneCallPanel : UIPanel
{
    
    public float ringTime = 2.5f;
    public float totalStandbyTime = 3.5f;
    public AudioSource audioSource;
    public AudioClip ringtone;
    public AudioClip answerSound;

    public void OnValidate()
    {
        if (totalStandbyTime < ringTime)
        {
            totalStandbyTime = ringTime + 0.1f;
        }
    }

    public override void Open()
    {
        base.Open();
        StartCoroutine(AwakeAnimation());
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
    }
}
