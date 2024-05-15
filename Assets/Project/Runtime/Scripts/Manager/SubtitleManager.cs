using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
   private void Awake()
   {
      
      RefreshContents();
   }

   public UnityEvent onDuplicateAdded;
   //public UnityEvent onDuplicateReveal;
   
   
   
   [Button("SetScrollRect")]
   public void SetScrollRect()
   {
      var scrollRect = transform.parent.GetComponent<ScrollRect>();
      scrollRect.verticalNormalizedPosition = 0;
   }

   public SubtitleContentElement templateSubtitleContentElement;
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
     // RefreshLayoutGroups.Refresh(gameObject);
      if (mostRecentDuplicate != null && mostRecentDuplicate.SubtitleText.ToString() == templateSubtitleContentElement.SubtitleText.ToString()) return;
      mostRecentDuplicate = Instantiate(templateSubtitleContentElement, duplicatedSubtitleContentContainer);
      mostRecentDuplicate.gameObject.SetActive(false);
      RefreshContents();
      
      mostRecentDuplicate.UpdateTime();
      
      onDuplicateAdded.Invoke();
      
   }
   
   public void RevealDuplicate()
   {
    
      if (mostRecentDuplicate == null || duplicatedSubtitleContentContainer.childCount == 0) return;
     // if (mostRecentDuplicate != null && mostRecentDuplicate.SubtitleText.ToString() ==
      //    templateSubtitleContentElement.SubtitleText.ToString()) return;
      
      mostRecentDuplicate.gameObject.SetActive(true);
      Debug.Log("revealing " + mostRecentDuplicate.SubtitleText.ToString());
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
