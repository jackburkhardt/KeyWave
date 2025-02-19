using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project;
using UnityEngine;
using UnityEngine.Events;

public abstract class ComponentSwitcher : MonoBehaviour
{
    public bool enableAllOnAwake;
    public bool rootsOnly = true;

    [HideIf("IsFirstComponentSwitcher")] public bool sync;
    
    [HideIf(EConditionOperator.Or, "enableAllOnAwake", "sync")]
    public int defaultIndex;


    [HideIf("sync")]
    public UnityEvent OnSwitch;

    [ShowIf("ShowTargetInInspector")]
    //[GetComponent]
    [SerializeField]
    private GameObject _target;
    
    public Action OnSwitched;


    
    public GameObject Target
    {
        get => _target == null ? gameObject : _target;
    }

    protected virtual bool ShowTargetInInspector => true;
    
    public abstract int ComponentCount { get; }
    
    
    public abstract int ActiveIndex { get; }
    
    private int _activeIndexOverride = -1;
    
    protected int activeIndexOverride
    {
        get
        {
            var index = _activeIndexOverride;
            _activeIndexOverride = -1;
            return index >= 0 ? index : ActiveIndex;
        } 
        set => _activeIndexOverride = value;
    }
    
    private bool IsNotFirstComponent => !IsFirstComponentSwitcher;

    [HideIf("sync")] [SerializeField] private bool _extras = false;

    protected bool ShowExtras => _extras && (IsFirstComponentSwitcher || !sync);

    [ShowIf("ShowExtras")] [GetComponent] public Animator animator;
    
    private bool AnimatorNotNull => animator != null && _extras;

    [ShowIf("AnimatorNotNull")] public string nextTriggerName = "Next";

    [ShowIf("AnimatorNotNull")] public string backTriggerName = "Back";

    [ShowIf("AnimatorNotNull")] public bool waitForAnimation = true;
    
    [ShowIf("ShowExtras")] public List<UnityEvent> OnSwitchedEvents;

    public bool IsFirstComponentSwitcher => FirstComponentSwitcher == this;

    private void Awake()
    {
        SwitchTo(defaultIndex);
    }

    public ComponentSwitcher FirstComponentSwitcher
    {
        get
        {
            var sisterComponents = GetComponentsInChildren<ComponentSwitcher>(true).Where(c => c.gameObject == gameObject);
            return sisterComponents.FirstOrDefault();
        }
    }
    
    
    public void Next()
    {
        if (waitForAnimation && AnimatorNotNull)
        {
            animator.SetTrigger(nextTriggerName);
            return;
        }
        
        SwitchToNextObject();
        OnSwitched?.Invoke();
    }
    
    public void Back()
    {
        if (waitForAnimation && AnimatorNotNull)
        {
            animator.SetTrigger(backTriggerName);
            return;
        }
        
        SwitchToPreviousObject();
        OnSwitched?.Invoke();
    }

     protected abstract void SwitchToNextObject();
 
       protected abstract void SwitchToPreviousObject();

       public abstract void SwitchTo(int index);

       public abstract void RevertTo(int index);


}

public abstract class ComponentSwitcher<T> : ComponentSwitcher
{
    private List<int> _walk;

    private void Awake()
    {
        _walk = new List<int>();
    }
   
    public override int ComponentCount => ComponentsToSwitch.Count;

    public int SyncedComponentCount
    {
        get
        {
            return sync ? FirstComponentSwitcher.ComponentCount : ComponentCount;
        }
    }

    private bool TryGetSisterComponentSwitchers(out List<Component> sisters)
    {
        var matchingComponents = new List<Component>();

        foreach (var component in gameObject.GetComponents<Component>())
        {
            if (component.GetType().BaseType.ToString().Contains("Switcher"))
            {
                matchingComponents.Add(component);
            }
        }
        
        sisters = matchingComponents;

        var type = typeof(ComponentSwitcher<>).GetGenericTypeDefinition();

        Debug.Log(GetComponents(typeof(ComponentSwitcher<>).GetGenericTypeDefinition()).Length);
        return matchingComponents.Count > 1;
    }
    
    int _queuedIndex = -1;


    protected abstract List<T> ComponentsToSwitch { get; }

    [HideIf("sync")]
    [Button("Next")]

    protected override void SwitchToNextObject()
    {
        var currentIndex = ActiveIndex;
        
        if (_walk != null) _walk.Add(ActiveIndex);
       
        var nextIndex = _queuedIndex >= 0 ? _queuedIndex
            : currentIndex + 1 >= ComponentsToSwitch.Count ? 0 : currentIndex + 1;

        _queuedIndex = -1;
        
      
        HideAll();
        ShowComponent(ComponentsToSwitch[nextIndex]);
        
        OnSwitch.Invoke();
        if (OnSwitchedEvents.Count > nextIndex)
        OnSwitchedEvents[nextIndex].Invoke();

        if (sync) return;
        
        foreach (var switcher in GetComponentsInChildren<ComponentSwitcher>(true).Where(
                     c => c.gameObject == gameObject
                     && c != this
                     && c.sync))
        {
            switcher.SwitchTo(nextIndex);
        }
        
    }
  
    [HideIf("sync")]
    [Button("Back")]

    protected override void SwitchToPreviousObject() {
        
        _queuedIndex = -1;
        var currentIndex = activeIndexOverride;
        var previousIndex = currentIndex - 1;
        
        if (_walk != null && _walk.Count != 0)
        {
            previousIndex = _walk[^1];
            _walk.RemoveAt(_walk.Count - 1);
        }
        
        else
        {
            if (previousIndex < 0) previousIndex = ComponentsToSwitch.Count - 1;
        }
       
        
        
        HideAll();
        ShowComponent(ComponentsToSwitch[previousIndex]);
        
        OnSwitch.Invoke();
        if (OnSwitchedEvents.Count > previousIndex) OnSwitchedEvents[previousIndex].Invoke();
        
        if (sync) return;
        
        foreach (var switcher in GetComponentsInChildren<ComponentSwitcher>(true).Where(
                     c => c.gameObject == gameObject
                          && c != this
                          && c.sync))
        {
            switcher.SwitchTo(previousIndex);
        }
        
    }

    public override void SwitchTo(int index)
    {
        if (index == ActiveIndex) return;
        _queuedIndex =
            index;
        Next();
    }
    
    public void SwitchTo(T obj)
    {
        _queuedIndex =
            ComponentsToSwitch.FindIndex(i => i.Equals(obj));
        if (_queuedIndex == ActiveIndex) return;
        Next();
    }
    
    public override void RevertTo(int index)
    {
        if (index == ActiveIndex) return;
        _walk = new List<int>();
        activeIndexOverride = index + 1;
        Back();
    }
    
    public void RevertTo(T obj)
    {
        var index = ComponentsToSwitch.FindIndex(i => i.Equals(obj));
        if (index == ActiveIndex) return;
        _walk = new List<int>();
        activeIndexOverride = index + 1;
        Back();
    }
    
    public void RevertToDefault()
    {
        RevertTo(defaultIndex);
    }
    
    

    public abstract void ShowComponent(T obj);

    public void HideAll()
    {
        foreach (var obj in ComponentsToSwitch)
        {
            HideComponent(obj);
        }
    }

    public abstract void HideComponent(T obj);

    private void OnValidate()
    {
        if (defaultIndex >= ComponentsToSwitch.Count)
        {
            defaultIndex = ComponentsToSwitch.Count - 1;
        }
        
        if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        
        if (OnSwitchedEvents.Count != ComponentsToSwitch.Count)
        {
            var oldEvents = OnSwitchedEvents;
            OnSwitchedEvents = new List<UnityEvent>();
            for (int i = 0; i < ComponentsToSwitch.Count; i++)
            {
                if (i < oldEvents.Count)
                {
                    OnSwitchedEvents.Add(oldEvents[i]);
                    for (var e = 0; e < OnSwitchedEvents[i].GetPersistentEventCount(); e++)
                    OnSwitchedEvents[i].SetPersistentListenerState(e, UnityEventCallState.EditorAndRuntime); }
                else
                {
                    OnSwitchedEvents.Add(new UnityEvent());
                }
            }
        }
        
    }
}
