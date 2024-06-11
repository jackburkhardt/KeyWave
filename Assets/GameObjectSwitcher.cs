using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectSwitcher : MonoBehaviour
{
    public bool enableAllOnAwake;
    
    public UnityEvent OnSwitch;
    private List<Transform> ObjectsToSwitch => GetComponentsInChildren<Transform>(true).ToList()
        .FindAll(t => t.gameObject != gameObject && t.parent == transform);
    
    private Transform ActiveObject => GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject != gameObject && t.gameObject.activeSelf && t.parent == transform);
  
    public void ShowObject(GameObject obj)
    {
        foreach (var o in ObjectsToSwitch)
        {
            if (o.gameObject == obj) o.gameObject.SetActive(true);
            else o.gameObject.SetActive(false);
            
        }

        OnSwitch.Invoke();
       
    }

    private void Awake()
    {
        if (enableAllOnAwake)
        {
            foreach (var obj in ObjectsToSwitch)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }

    public void HideObject(GameObject obj)
    {
        obj.SetActive(false);
    }
    
    public void HideAll()
    {
        foreach (var obj in ObjectsToSwitch)
        {
            obj.gameObject.SetActive(false);
        }
    }
    
    [Button("Next")]
    
    private void SwitchToNextObject()
    {
        if (ActiveObject == null)
        {
            ObjectsToSwitch[0].gameObject.SetActive(true);
     
            return;
        }

        var currentIndex = ActiveObject.GetSiblingIndex();
       
        var nextIndex = currentIndex + 1;
      
        if (nextIndex >= ObjectsToSwitch.Count) nextIndex = 0;
       
        ShowObject(ObjectsToSwitch[nextIndex].gameObject);
       
        
    }

    [Button("Previous")]

    private void SwitchToPreviousObject()
    {
        if (ActiveObject == null)
        {
            ObjectsToSwitch[0].gameObject.SetActive(true);
            return;
        }
        
        var currentIndex = ActiveObject.GetSiblingIndex();
        var previousIndex = currentIndex - 1;
        if (previousIndex < 0) previousIndex = ObjectsToSwitch.Count - 1;
        ShowObject(ObjectsToSwitch[previousIndex].gameObject);
    }
    
}
