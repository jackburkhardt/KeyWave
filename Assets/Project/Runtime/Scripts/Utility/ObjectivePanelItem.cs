using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Utility
{
    public class ObjectivePanelItem : MonoBehaviour
    {
        public TMP_Text questTitle;
        public Image tickImage, boxImage;

        [Foldout("Active")] [Label("Text Color")] public Color textColorActive;
        [Foldout("Active")] [Label("Tick Sprite")] public Sprite tickSpriteActive;

        [Foldout("Failure")] [Label("Text Color")] public Color textColorFailure;
        [Foldout("Failure")] [Label("Tick Sprite")] public Sprite tickSpriteFailure;

        [Foldout("Success")] [Label("Text Color")] public Color textColorSuccess;
        [Foldout("Success")] [Label("Tick Sprite")] public Sprite tickSpriteSuccess;

        public void SetVisibleElements(string state, string questDesc)
        {
            questTitle.text = questDesc.Split('.')[0];
            switch (state)
            {
                case "active":
                    questTitle.color = textColorActive;
                    tickImage.sprite = tickSpriteActive;
                    break;
                case "failure":
                    questTitle.color = textColorFailure;
                    tickImage.sprite = tickSpriteFailure;
                    break;
                case "success":
                    questTitle.color = textColorSuccess;
                    tickImage.sprite = tickSpriteSuccess;
                    break;
            }
        }
    }
}