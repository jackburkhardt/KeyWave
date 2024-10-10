using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CursorPositionContextualButton : Selectable, IPointerClickHandler, ISubmitHandler
{
   
   [Serializable]
         
   public class ButtonClickedEvent : UnityEvent {}
  
          // Event delegates triggered on click.
          [FormerlySerializedAs("onClickLeft")]
          [SerializeField]
          private ButtonClickedEvent m_OnClickLeft = new ButtonClickedEvent();
          
          [FormerlySerializedAs("onClickRight")]
          [SerializeField]
          private ButtonClickedEvent m_OnClickRight = new ButtonClickedEvent();
  
          protected CursorPositionContextualButton()
          {}
  
          public ButtonClickedEvent onClickLeft
          {
              get { return m_OnClickLeft; }
              set { m_OnClickLeft = value; }
          }
          
          public ButtonClickedEvent onClickRight
          {
              get { return m_OnClickRight; }
              set { m_OnClickRight = value; }
          }
  
          private void Press(Vector2 relativePosition)
          {
              
              
              if (!IsActive() || !IsInteractable())
                  return;
              
              if (relativePosition.x < 0)
              {
                  m_OnClickLeft.Invoke();
              }
              
              else m_OnClickRight.Invoke();
  
              UISystemProfilerApi.AddMarker("Button.onClick", this);
             
          }
  
     
          public virtual void OnPointerClick(PointerEventData eventData)
          {
              if (eventData.button != PointerEventData.InputButton.Left)
                  return;

              var mousePosition = Input.mousePosition;
              var rectTransform = targetGraphic == null ? GetComponent<RectTransform>() : targetGraphic.rectTransform;
              var canvas = this.GetComponentInParent<Canvas>();
              
              RectTransformUtility.ScreenPointToLocalPointInRectangle(
                  canvas.transform as RectTransform,
                  Input.mousePosition, canvas.worldCamera,
                  out Vector2 canvasPosition);
              
              var buttonArea = RectTransformUtility.PixelAdjustRect(rectTransform, this.GetComponentInParent<Canvas>());
              var relativePosition = new Vector2((buttonArea.x * 2- canvasPosition.x)/buttonArea.x, (buttonArea.y * 2 - canvasPosition.y)/buttonArea.y);
              
            
              Press(relativePosition);
          }
          
          public virtual void OnSubmit(BaseEventData eventData)
          {
              Press(new Vector2(0, 0));
  
              // if we get set disabled during the press
              // don't run the coroutine.
              if (!IsActive() || !IsInteractable())
                  return;
  
              DoStateTransition(SelectionState.Pressed, false);
              StartCoroutine(OnFinishSubmit());
          }
  
          private IEnumerator OnFinishSubmit()
          {
              var fadeTime = colors.fadeDuration;
              var elapsedTime = 0f;
  
              while (elapsedTime < fadeTime)
              {
                  elapsedTime += Time.unscaledDeltaTime;
                  yield return null;
              }
  
              DoStateTransition(currentSelectionState, false);
          }
}
