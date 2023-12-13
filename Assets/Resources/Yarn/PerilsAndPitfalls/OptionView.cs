using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Yarn.Unity;

namespace PerilsAndPitfalls
{
    public class OptionView : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] AudioClip hoverSound;
        [SerializeField] AudioClip selectionSound;

        
        [SerializeField] TextMeshProUGUI _text;
        Color m_textColor;
        [SerializeField] Color _inactiveOptionColor = new Color(0.6f, 0.6f, 0.6f, 1);
        [SerializeField] Color _dismissedOptionColor = new Color(0.2f, 0.2f, 0.2f, 1);
        [SerializeField] bool _showCharacterName = false;
        [SerializeField] float _textSpaceOffset;

        public Action<DialogueOption> OnOptionSelected;

        DialogueOption _option;

        bool hasSubmittedOptionSelection = false;

        public DialogueOption Option
        {
            get => _option;

            set
            {
                m_textColor = _text.color;
                _text.color = _inactiveOptionColor;
                _option = value;

                hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                if (_showCharacterName)
                {
                    _text.text = value.Line.Text.Text;
                }
                else
                {
                    _text.text = value.Line.TextWithoutCharacterName.Text;
                }
                interactable = value.IsAvailable;
            }
        }

        // If we receive a submit or click event, invoke our "we just selected
        // this option" handler.
        public void OnSubmit(BaseEventData eventData)
        {

            InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (hasSubmittedOptionSelection == false)
            {
                OnOptionSelected.Invoke(Option);
                if (selectionSound != null) GetComponent<AudioSource>().PlayOneShot(selectionSound);
                hasSubmittedOptionSelection = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            InvokeOptionSelected();
        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
            if (!interactable) return;

            if (hoverSound != null) GetComponent<AudioSource>().PlayOneShot(hoverSound);

            _text.fontStyle = FontStyles.Bold;
            _text.characterSpacing -= _textSpaceOffset;
            _text.color = m_textColor;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;

            _text.fontStyle = FontStyles.Normal;
            _text.characterSpacing += _textSpaceOffset;
            _text.color = _inactiveOptionColor;

        }
    }
}
