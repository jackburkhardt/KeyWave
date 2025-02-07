using System.Collections;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

public class MapCafeButton : MonoBehaviour
{
    [SerializeField] private RectTransform _mapButtonTemplateContainer;
    
    Button _button;
    
    
    [Button]
    public void SetProperties()
    {
        gameObject.SetActive(true);
        
        var cafeButton = _mapButtonTemplateContainer.GetComponentsInChildren<Transform>().ToList()
            .Find(p => p.gameObject.name.Contains("Café"));

        if (cafeButton == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        _button = cafeButton.GetComponent<Button>();

        if (_button.interactable == false)
        {
            gameObject.SetActive(false);
            return;
        }
        

        var animator = cafeButton.GetComponent<Animator>();


        if (animator != null)
        {
            animator.SetBool("Hide", true);
            animator.SetTrigger("Normal");
        }
    }

    public void SimulateClick()
    {
        ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
       // ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current),
       //     ExecuteEvents.pointerEnterHandler);
     //   ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current),
         //   ExecuteEvents.pointerDownHandler);
        
        StartCoroutine(DelayedSelect());
        
        IEnumerator DelayedSelect()
        {
            yield return new WaitForSeconds(0.05f);
            
            ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
           // ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current),
              //  ExecuteEvents.pointerUpHandler);
            
           // ExecuteEvents.Execute(_button.gameObject, new PointerEventData(EventSystem.current),
              //  ExecuteEvents.selectHandler);
        }
       // ExecuteEvents.Execute(_button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.selectHandler);
    }
    
    // Start is called before the first frame update

}
