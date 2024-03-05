using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// A "window" created by a PopupItem.
    /// </summary>
    public class PopUpWindow : MonoBehaviour
    {
        [SerializeField] private GameObject _imageView;
        [SerializeField] private GameObject _textView;
        public bool TextViewActive;

        private void Awake()
        {
            GameEvent.OnPopupViewChange += ToggleTextView;
        }

        public void Discard()
        {
            Destroy(this.gameObject);
        }

        private void ToggleTextView()
        {
            // there are two popups active, but only one is shown. this switches between which is visible
            if (!TextViewActive)
            {
                TextViewActive = true;
                _textView.transform.position = _imageView.transform.position;
                _imageView.SetActive(false);
                _textView.SetActive(true);
            }
            else
            {
                TextViewActive = false;
                _imageView.transform.position = _textView.transform.position;
                _textView.SetActive(false);
                _imageView.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnPopupViewChange -= ToggleTextView;
        }
    }
}
