using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Manager
{
    public class SubtitleManager : MonoBehaviour
    {
        public UnityEvent onDuplicateAdded;

        public SubtitleContentElement templateSubtitleContentElement;
        public TextMeshProUGUI templateResponseDuplicate;
        public Transform duplicatedSubtitleContentContainer;
        private SubtitleContentElement mostRecentDuplicate;
        private Subtitle mostRecentSubtitle;

        private void Awake()
        {
      
            RefreshContents();
            templateResponseDuplicate.gameObject.SetActive(false);
        }
        //public UnityEvent onDuplicateReveal;


        [Button("SetScrollRect")]
        public void SetScrollRect()
        {
            var scrollRect = transform.parent.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 0;
        }

        public void ClearContents()
        {
            for (int i = 0; i < duplicatedSubtitleContentContainer.childCount; i++)
            {
                if (duplicatedSubtitleContentContainer.GetChild(i).gameObject != templateResponseDuplicate.gameObject)
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

        public void AddResponse(TextMeshProUGUI button)
        {
            var response = Instantiate(templateResponseDuplicate, templateResponseDuplicate.gameObject.transform.parent);
            response.gameObject.SetActive(true);
            response.text = button.text;

        }

        public void RevealDuplicate()
        {
    
            if (mostRecentDuplicate == null || duplicatedSubtitleContentContainer.childCount == 0) return;
            mostRecentDuplicate.gameObject.SetActive(true);
            RefreshContents();
        }


        private void OnConversationLine(Subtitle subtitle)
        {
            RefreshContents();
        }
    }
}