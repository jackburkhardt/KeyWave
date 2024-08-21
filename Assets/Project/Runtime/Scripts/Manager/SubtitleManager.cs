using System.Collections.Generic;
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
            templateSubtitleContentElement.gameObject.SetActive(false);
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
                if (duplicatedSubtitleContentContainer.GetChild(i).gameObject ==
                    templateResponseDuplicate.gameObject) continue;
                if (duplicatedSubtitleContentContainer.GetChild(i).gameObject == templateSubtitleContentElement.gameObject) continue;
                    Destroy(duplicatedSubtitleContentContainer.GetChild(i).gameObject);
            }
            
            _queuedSubtitles.Clear();
      
            templateSubtitleContentElement.Clear();
            
            
      
        }

        public void RefreshContents()
        {
            if (!templateSubtitleContentElement.PortraitActive) templateSubtitleContentElement.HidePortraitHelper();
            else templateSubtitleContentElement.ShowPortraitHelper();
            RefreshLayoutGroups.Refresh(gameObject);
        }

        private List<SubtitleContentElement> _queuedSubtitles = new List<SubtitleContentElement>();
        
        private void RevealQueuedSubtitles()
        {
            if (_queuedSubtitles.Count == 0) return;
            foreach (var subtitle in _queuedSubtitles)
            {
                subtitle.gameObject.SetActive(true);
            }
            _queuedSubtitles.Clear();
            RefreshContents();
        }

        public void AddHiddenSubtitle(SubtitleContentElement subtitleElement)
        {
            if (subtitleElement.SubtitleText == string.Empty) return;
            RevealQueuedSubtitles();
            var duplicate = Instantiate(templateSubtitleContentElement, duplicatedSubtitleContentContainer);
            duplicate.Initialize(subtitleElement);
            _queuedSubtitles.Add(duplicate);
            RefreshContents();
            onDuplicateAdded.Invoke();
        }

        public void AddResponse(TextMeshProUGUI button)
        {
            var response = Instantiate(templateResponseDuplicate, templateResponseDuplicate.gameObject.transform.parent);
            response.gameObject.SetActive(true);
            response.text = button.text;

        }

        private void OnConversationLine(Subtitle subtitle)
        {
            RefreshContents();
        }
    }
}