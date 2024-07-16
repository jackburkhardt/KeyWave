using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIMenuPanel : StandardUIMenuPanel
    {
        public static List<string> CustomFields = new List<string>
        {
       
        };


        [SerializeField] Animator responseMenuAnimator;


        public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();


        protected override void OnEnable()
        { 
            base.OnEnable();
            onContentChanged.AddListener(OnContentChanged);
        }

        protected override void OnDisable()
        { 
            base.OnDisable();
            onContentChanged.RemoveListener(OnContentChanged);
        }

        protected virtual void OnContentChanged()
        {
            var responseButtons = GetComponentsInChildren<CustomUIResponseButton>();
            foreach (var button in responseButtons)
            {
                if (!button.gameObject.activeSelf && button != buttonTemplate) return;
                button.Refresh();
            }
        }

        public void OnQuestStateChange(string questTitle)
        {
       
        }

        public void OnConversationEnd()
        {
     
        }
    }
}