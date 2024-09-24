using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameObjectSwitcher : MonoBehaviour
{
    public bool enableAllOnAwake;
    public bool rootsOnly = true;

  
    
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
        .FindAll(t => t.gameObject != gameObject && (t.parent == transform || !rootsOnly));
    
    private List<Image> ImagesToSwitch => GetComponentsInChildren<Image>(true).ToList()
        .FindAll(i => i.gameObject != gameObject && (i.transform.parent == transform || !rootsOnly));
    
    private List<CanvasGroup> CanvasGroupsToSwitch => GetComponentsInChildren<CanvasGroup>(true).ToList()
        .FindAll(i => i.gameObject != gameObject && (i.transform.parent == transform || !rootsOnly));

    [SerializeField] private bool _extras = false;
    
    public bool ShowImageComponentSwitcher => _extras && !canvasGroupSwitcher;
    
    public bool ShowCanvasGroupSwitcher => _extras && !imageComponentSwitcher;
    
    [ShowIf("_extras")] 
    [GetComponent]
    public Animator animator;
    
    [ShowIf("AnimatorNotNull")] 
    public string nextTriggerName = "Next";
    
    [ShowIf("AnimatorNotNull")] 
    public string backTriggerName = "Back";

    [ShowIf("AnimatorNotNull")] public bool waitForAnimation = true;
    private bool AnimatorNotNull => animator != null;

    [ShowIf("ShowImageComponentSwitcher")] public bool imageComponentSwitcher;
    
    private Transform ActiveObject => GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject != gameObject && t.gameObject.activeSelf && t.parent == transform);
    
    private Image ActiveImage => ImagesToSwitch.FirstOrDefault(i => i.enabled);
    
    private CanvasGroup ActiveCanvasGroup => CanvasGroupsToSwitch.FirstOrDefault(i => (int)i.GetComponent<CanvasGroup>().alpha == 1);
    
    [ShowIf("ShowCanvasGroupSwitcher")] public bool canvasGroupSwitcher;
    
  
   
    int _queuedIndex = -1;
  
    public void ShowObject(GameObject obj)
    {
        if (imageComponentSwitcher)
        {
            _queuedIndex = ImagesToSwitch.FindIndex(i => i.gameObject == obj);
        }
        
        else if(canvasGroupSwitcher)
        {
            _queuedIndex = CanvasGroupsToSwitch.FindIndex(i => i.gameObject == obj);
        }

        else
        {
            _queuedIndex = ObjectsToSwitch.FindIndex(i => i.gameObject == obj);
        }

        
    }

    private void Awake()
    {
        _walk = new List<int>();
        
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
            
            else if (canvasGroupSwitcher)
            {
                foreach (var obj in ObjectsToSwitch)
                {
                    obj.gameObject.SetActive(obj.GetComponent<CanvasGroup>() != null);
                }

                foreach (var i in CanvasGroupsToSwitch)
                {
                    i.GetComponent<CanvasGroup>().alpha = 1;
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
        
        if (canvasGroupSwitcher && currentValidation != "canvas")
        {
            foreach (var obj in ObjectsToSwitch)
            {
                obj.gameObject.SetActive(obj.GetComponent<CanvasGroup>() != null);
            }
            
            currentValidation = "canvas";
            
            ShowObject(CanvasGroupsToSwitch[0].gameObject);
        }
    }

    public void HideObject(GameObject obj)
    {
        if (imageComponentSwitcher) obj.GetComponent<Image>().enabled = false;
        else if (canvasGroupSwitcher) obj.GetComponent<CanvasGroup>().alpha = 0;
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
        
        else if (canvasGroupSwitcher)
        {
            foreach (var i in CanvasGroupsToSwitch)
            {
                i.GetComponent<CanvasGroup>().alpha = 0;
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



    public void SwitchTo(GameObject obj)
    {
        _queuedIndex = 
            imageComponentSwitcher ? ImagesToSwitch.FindIndex(i => i.gameObject == obj)
                : canvasGroupSwitcher ? CanvasGroupsToSwitch.FindIndex(i => i.gameObject == obj)
            : ObjectsToSwitch.FindIndex(i => i.gameObject == obj);
        
        Next();
    }


   

    public void Next()
    {
        if (waitForAnimation && AnimatorNotNull)
        {
            animator.SetTrigger(nextTriggerName);
            return;
        }
        
        SwitchToNextObject();
    }

    private List<int> _walk;
    
    [Button("Next")]
    
    private void SwitchToNextObject()
    {
        if (imageComponentSwitcher && ActiveImage == null)
        {
            ImagesToSwitch[0].enabled = true;
            return;
        }
        
        if (canvasGroupSwitcher && ActiveCanvasGroup == null)
        {
            CanvasGroupsToSwitch[0].GetComponent<CanvasGroup>().alpha = 1;
            return;
        }
        
        if (ActiveObject == null)
        {
            ObjectsToSwitch[0].gameObject.SetActive(true);
            return;
        }

        var currentIndex =
            imageComponentSwitcher ? ImagesToSwitch.IndexOf(ActiveImage)
            : canvasGroupSwitcher ? CanvasGroupsToSwitch.IndexOf(ActiveCanvasGroup)
            : ObjectsToSwitch.IndexOf(ActiveObject);
        
        if (_walk != null) _walk.Add(currentIndex);

        var nextIndex = _queuedIndex > 0 ? _queuedIndex
            : imageComponentSwitcher ? currentIndex + 1 >= ImagesToSwitch.Count ? 0 : currentIndex + 1
            : canvasGroupSwitcher ? currentIndex + 1 >= CanvasGroupsToSwitch.Count? 0 : currentIndex + 1
            : currentIndex + 1 >= ObjectsToSwitch.Count ? 0 : currentIndex + 1;
        
        _queuedIndex = -1;
        
        if (imageComponentSwitcher)
        {
            ActiveImage.enabled = false;
            ImagesToSwitch[nextIndex].enabled = true;
        }
        
        else if (canvasGroupSwitcher)
        {
            ActiveCanvasGroup.GetComponent<CanvasGroup>().alpha = 0;
            CanvasGroupsToSwitch[nextIndex].GetComponent<CanvasGroup>().alpha = 1;
        }
        
        else
        {
            ActiveObject.gameObject.SetActive(false);
            ObjectsToSwitch[nextIndex].gameObject.SetActive(true);
        }
        
        OnSwitch.Invoke();
    }



   

    public void Back()
    {
        if (waitForAnimation && AnimatorNotNull)
        {
            animator.SetTrigger(backTriggerName);
            return;
        }
        
        SwitchToPreviousObject();
    }
    
    [Button("Back")]

    private void SwitchToPreviousObject()
    {
        var currentIndex =
            imageComponentSwitcher ? ImagesToSwitch.IndexOf(ActiveImage)
            : canvasGroupSwitcher ? CanvasGroupsToSwitch.IndexOf(ActiveCanvasGroup)
            : ObjectsToSwitch.IndexOf(ActiveObject);
        
        if (imageComponentSwitcher && ActiveImage != null)
        {
            ActiveImage.enabled = false;
        }
        
        else if (canvasGroupSwitcher && ActiveCanvasGroup != null)
        {
            ActiveCanvasGroup.GetComponent<CanvasGroup>().alpha = 0;
        }
        
        else if (ActiveObject != null)
        {
            ActiveObject.gameObject.SetActive(false);
        }
        
        
        
     
        
        var previousIndex = currentIndex - 1;

        if (_walk != null && _walk.Count != 0)
        {
            previousIndex = _walk[^1];
            _walk.RemoveAt(_walk.Count - 1);
        }
        
        else
        {
            if (previousIndex < 0) previousIndex = imageComponentSwitcher ? ImagesToSwitch.Count - 1
                : canvasGroupSwitcher ? CanvasGroupsToSwitch.Count - 1
                : ObjectsToSwitch.Count - 1;
        }
        
        if (imageComponentSwitcher)
        {
            ImagesToSwitch[previousIndex].enabled = true;
        }
        
        else if (canvasGroupSwitcher)
        {
            CanvasGroupsToSwitch[previousIndex].GetComponent<CanvasGroup>().alpha = 1;
        }
        
        else
        {
           ObjectsToSwitch[previousIndex].gameObject.SetActive(true);
        }
       
    }

    
}
