using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SublocationBackground : MonoBehaviour
{
    [InfoBox("This script will fade the Image component if the Dialogue System Conversation Title contains the sublocation name.")]
    public string sublocationName;
   
    [ShowIf("ContainsImage")]
    [ReadOnly] [SerializeField] private string _image = "Using Image component attached to GameObject";
    [ShowIf("ContainsImage")]
    [Label("Override?")]
    public bool overrideImage = false;
    [HideIf("ContainsImageAndNotOverriden")]
    public Image image;
    
    [SerializeField] private float fadeDuration = 1f;

    private bool ContainsImage => GetComponent<Image>() != null;
    private bool ContainsImageAndNotOverriden => ContainsImage && !overrideImage;

    private void OnEnable()
    {
       // DialogueSystemStaticEvents.OnConversationLineEvent += OnConversationLine;
    }

    private void OnConversationLine(Subtitle subtitle)
    {
        var conversationTitle = subtitle.dialogueEntry;
    }
    

}
