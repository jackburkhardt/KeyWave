#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIButtonKeyTrigger = PixelCrushers.UIButtonKeyTrigger;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class CustomUIResponseButtonChameleon : CustomUIResponseButton
    {
        //private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
        
      
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        public void ClearChameleon()
        {
            
            response = null;
            label.text = "";
            Icon.sprite = null;
            nodeColorChameleon.color = defaultImageColor;
            actorNameText.text = "";
            conversantNameText.text = "";

            GetComponent<Animator>().SetTrigger("Hide");

        }
        
        public void Chameleonize(CustomUIResponseButton button)
        {
            if (button.label.text.Contains("Metrics")) return;
            response = button.response;
            
            label.text = button.label.text;
            Icon.sprite = button.Icon.sprite;
            nodeColorChameleon.color = button.NodeColorChameleon.color;
            actorNameText.text = button.ActorName;
            conversantNameText.text = button.ConversantName;
            defaultImageColor = button.DefaultImageColor;
        }

        public void GoToResponse()
        {
            var entry = response.destinationEntry.GetNextDialogueEntry();
            if (DialogueManager.instance == null || DialogueManager.instance.conversationModel == null)
            {
                var nextConversation = entry.GetConversation();
                DialogueManager.StartConversation(nextConversation.Title);
            }

            else
            {
                var state = DialogueManager.instance.conversationModel.GetState(entry);
                DialogueManager.conversationController.GotoState(state);
            }
        }


    }
}