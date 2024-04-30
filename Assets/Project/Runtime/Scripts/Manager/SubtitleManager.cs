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
      mostRecentDuplicate = Instantiate(templateSubtitleContentElement, duplicatedSubtitleContentContainer);
      mostRecentDuplicate.gameObject.SetActive(false);
     RefreshContents();
   }
   
   public void RevealDuplicate()
   {
      if (mostRecentDuplicate == null || duplicatedSubtitleContentContainer.childCount == 0) return;
      mostRecentDuplicate.gameObject.SetActive(true);
      RefreshContents();
   }
   
   private void OnConversationLineEnd(Subtitle subtitle)
   {
      RefreshLayoutGroups.Refresh(gameObject);
      if (subtitle.formattedText.text == string.Empty) return;
      if (CustomResponsePanel.SelectedResponseButton != null && subtitle.dialogueEntry == CustomResponsePanel.SelectedResponseButton.DestinationEntry) return;
      if (CustomResponsePanel.SelectedResponseButton != null) Debug.Log($"CustomResponsePanel = {CustomResponsePanel.SelectedResponseButton.DestinationEntry.id}, subtitle.dialogueEntry = {subtitle.dialogueEntry.id}");
      AddHiddenDuplicate();
      mostRecentDuplicate.UpdateTime();
   }

   private void OnConversationLine(Subtitle subtitle)
   {
      if (subtitle.formattedText.text == string.Empty) return;
      RevealDuplicate();
   }
   
}
