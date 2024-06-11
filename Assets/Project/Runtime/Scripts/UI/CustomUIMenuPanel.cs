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
            foreach (var button in ResponseButtons)
            {
                if (!button.gameObject.activeSelf && button != buttonTemplate) Destroy(button);
                else button.Refresh();
            }
        }

        public void OnQuestStateChange(string questTitle)
        {
       
        }

        public void OnConversationEnd()
        {
            responseMenuAnimator.SetTrigger("Hide");
        }
    }
}