using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
   private void Awake()
   {
      
      RefreshContents();
   }

   [SerializeField] private SubtitleContentElement templateSubtitleContentElement;
   public Transform duplicatedSubtitleContentContainer;
   private SubtitleContentElement mostRecentDuplicate;
   private Subtitle mostRecentSubtitle;
   
   public void ClearContents()
   {
      for (int i = 0; i < duplicatedSubtitleContentContainer.childCount; i++)
      {
         Destroy(duplicatedSubtitleContentContainer.GetChild(i).gameObject);
      }
      
      templateSubtitleContentElement.Clear();
      
   }
   
   public void RefreshContents()
   {
      if (!templateSubtitleContentElement.PortraitActive) templateSubtitleContentElement.HidePortraitHelper();
      else templateSubtitleContentElement.ShowPortraitHelper();
      RefreshLayoutGroups.Refresh(gameObject);
      
   }

   public void AddHiddenDuplicate()
   {
      RefreshLayoutGroups.Refresh(gameObject);
      //if (subtitle.formattedText.text == string.Empty) return;
   //   if (CustomUIMenuPanel.SelectedResponseButton != null && subtitle.dialogueEntry == CustomUIMenuPanel.SelectedResponseButton.DestinationEntry) return;
   //   if (mostRecentDuplicate != null && mostRecentDuplicate.SubtitleText.ToString() == subtitle.formattedText.text) return;
      mostRecentDuplicate = Instantiate(templateSubtitleContentElement, duplicatedSubtitleContentContainer);
      mostRecentDuplicate.gameObject.SetActive(false);
      RefreshContents();
      
      mostRecentDuplicate.UpdateTime();
      
   }
   
   public void RevealDuplicate()
   {
      if (mostRecentDuplicate == null || duplicatedSubtitleContentContainer.childCount == 0) return;
      if (mostRecentDuplicate != null && mostRecentDuplicate.SubtitleText.ToString() ==
          templateSubtitleContentElement.SubtitleText.ToString()) return;
      mostRecentDuplicate.gameObject.SetActive(true);
      RefreshContents();
   }
   
   /*
   private void OnConversationLineEnd(Subtitle subtitle)
   {
     
      AddHiddenDuplicate();
      
   }
   */

   private void OnConversationLine(Subtitle subtitle)
   {
      RefreshContents();
   }
   
}
