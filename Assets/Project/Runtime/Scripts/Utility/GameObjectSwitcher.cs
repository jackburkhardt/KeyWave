using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameObjectSwitcher : MonoBehaviour
{
    public bool enableAllOnAwake;
    
    [HideIf("enableAllOnAwake")]
    [ReadOnly] public GameObject defaultObject;
    
    [HideIf("enableAllOnAwake")]
    [Button("Set Default")]
    private void SetDefaultObject()
    {
        defaultObject = ActiveObject.gameObject;
    }
    
    public UnityEvent OnSwitch;
    private List<Transform> ObjectsToSwitch => GetComponentsInChildren<Transform>(true).ToList()
        .FindAll(t => t.gameObject != gameObject && t.parent == transform);
    
    private List<Image> ImagesToSwitch => GetComponentsInChildren<Image>(true).ToList()
        .FindAll(i => i.gameObject != gameObject && i.transform.parent == transform);

    public bool extras = false;

    [ShowIf("extras")] public bool imageComponentSwitcher;
    
    private Transform ActiveObject => GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject != gameObject && t.gameObject.activeSelf && t.parent == transform);
    
    private Image ActiveImage => ImagesToSwitch.FirstOrDefault(i => i.enabled);
    
  
   
  
    public void ShowObject(GameObject obj)
    {
        if (imageComponentSwitcher)
        {
            foreach (var i in ImagesToSwitch)
            {
                if (i.gameObject == obj) i.enabled = true;
                else i.enabled = false;
            }
        }

        else
        {
            foreach (var o in ObjectsToSwitch)
            {
                if (o.gameObject == obj) o.gameObject.SetActive(true);
                else o.gameObject.SetActive(false);
            
            }
        }

        OnSwitch.Invoke();
    }

    private void Awake()
    {
        if (enableAllOnAwake)
        {
            if (imageComponentSwitcher)
            {
                foreach (var obj in ObjectsToSwitch)
                {
                    obj.gameObject.SetActive(obj.GetComponent<Image>() != null);
                }

                foreach (var i in ImagesToSwitch)
                {
                    i.enabled = true;
                }
            }

            else
            {
                foreach (var obj in ObjectsToSwitch)
                {
                    obj.gameObject.SetActive(true);
                }
            }
        }

        else if (defaultObject != null)
        {
            ShowObject(defaultObject);
        }
      
    }

    private string currentValidation = "";

    private void OnValidate()
    {
        if (defaultObject == null) defaultObject = ObjectsToSwitch[0].gameObject;
        
        if (imageComponentSwitcher && currentValidation != "image")
        {
            foreach (var obj in ObjectsToSwitch)
            {
                obj.gameObject.SetActive(obj.GetComponent<Image>() != null);
            }
            
            currentValidation = "image";
            
            ShowObject(ImagesToSwitch[0].gameObject);
            
        }
        
        if (!imageComponentSwitcher && currentValidation != "object")
        {
            
            foreach (var i in ImagesToSwitch)
            {
                i.enabled = true;
            }
            
            currentValidation = "object";
            
            ShowObject(defaultObject);
        }
    }

    public void HideObject(GameObject obj)
    {
        if (imageComponentSwitcher) obj.GetComponent<Image>().enabled = false;
        else obj.SetActive(false);
    }
    
    public void HideAll()
    {
        if (imageComponentSwitcher)
        {
            foreach (var i in ImagesToSwitch)
            {
                i.enabled = false;
            }
        }
        else
        {
            foreach (var obj in ObjectsToSwitch)
            {
                obj.gameObject.SetActive(false);
            }
        }
    }

   
    
   
    
    [Button("Next")]
    
    private void SwitchToNextObject()
    {
        if (imageComponentSwitcher)
        {
            if (ActiveImage == null)
            {
                ImagesToSwitch[0].enabled = true;
                return;
            }
            
            var currentIndex = ActiveImage.transform.GetSiblingIndex();
            var nextIndex = currentIndex + 1;
            if (nextIndex >= ImagesToSwitch.Count) nextIndex = 0;
            
            ShowObject(ImagesToSwitch[nextIndex].gameObject);
        }

        else
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
