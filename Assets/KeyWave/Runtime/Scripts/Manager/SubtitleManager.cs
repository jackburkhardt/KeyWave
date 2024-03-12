using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
   [SerializeField] private SubtitleContentElement subtitleContentElement;
   public Transform duplicatedSubtitleContentContainer;
   private SubtitleContentElement mostRecentSubtitleContentElement;
   
   public void ClearContents()
   {
      for (int i = 0; i < duplicatedSubtitleContentContainer.childCount; i++)
      {
         Destroy(duplicatedSubtitleContentContainer.GetChild(i).gameObject);
      }
   }

   public void AddSubtitleContent()
   {
      mostRecentSubtitleContentElement = Instantiate(subtitleContentElement, duplicatedSubtitleContentContainer);
      mostRecentSubtitleContentElement.gameObject.SetActive(false);
   }
   
   public void RevealSubtitleContent()
   {
      if (mostRecentSubtitleContentElement == null) return;
      mostRecentSubtitleContentElement.gameObject.SetActive(true);
   }
   
}
