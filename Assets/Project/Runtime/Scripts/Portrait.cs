using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts
{
    public class Portrait : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private Image image;
        private Image currentImage => GetComponent<Image>();

        private void OnEnable()
        {
            animator.SetTrigger("ShowPortrait");
        }

        private void OnDisable()
        {
            animator.SetTrigger("HidePortrait");
        }


        private void OnConversationLine()
        {
            if (currentImage.sprite == null)
            {
                if (image != null) currentImage.sprite = image.sprite;
            }
      
            else image = currentImage;
        }
    }
}