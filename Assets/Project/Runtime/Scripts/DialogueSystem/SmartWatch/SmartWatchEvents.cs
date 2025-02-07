using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class SmartWatchEvents : MonoBehaviour
{
    
    [ShowIf("ShowAnimatorTrigger")]
    [ReadOnly] [SerializeField] private Animator animator;
    
    public enum ActionFlags
    {
        None = 0,
        UnityEvent = 1 << 0,
        AnimatorTrigger = 1 << 1
    }
    
    [EnumFlags] public ActionFlags actions;
    
    [SerializeReference] private List<SmartWatchApp> apps;
   
    private bool ShowUnityEvent => actions.HasFlag(ActionFlags.UnityEvent);
    private bool ShowAnimatorTrigger => actions.HasFlag(ActionFlags.AnimatorTrigger);

    private void OnEnable()
    {
        SmartWatch.OnAppOpen += OnAppOpen;
    }
    
    private void OnDisable()
    {
        SmartWatch.OnAppOpen -= OnAppOpen;
    }


    [Serializable]
    public class SmartWatchApp
    {
        [HideInInspector] [ReadOnly] public string name;
        [HideInInspector] [ReadOnly] public SmartWatch.App app;
        
        private bool _showUnityEvent;
        private bool _showAnimatorTrigger;
        
        private Animator animator;
        
        [ShowIf("_showUnityEvent")]
        [AllowNesting]
        [SerializeField] public UnityEvent onAppOpen;
        
        [ShowIf("_showAnimatorTrigger")]
        [AnimatorParam("animator")]
        [AllowNesting]
        [SerializeField] public string animatorTrigger;
        
        public SmartWatchApp(SmartWatch.App app, SmartWatchEvents parent)
        {
            this.app = app;
            name = app.name;
            _showUnityEvent = parent.ShowUnityEvent;
            _showAnimatorTrigger = parent.ShowAnimatorTrigger;
            animator = parent.animator;
        }
        
        public void SetApp(SmartWatch.App app,  SmartWatchEvents parent)
        {
            this.app = app;
            name = app.name;
            _showUnityEvent = parent.ShowUnityEvent;
            _showAnimatorTrigger = parent.ShowAnimatorTrigger;
            animator = parent.animator;
        }
    }
    
    private void OnValidate()
    {
        animator ??= GetComponent<Animator>();
        if (SmartWatch.instance == null || SmartWatch.instance.apps == null) return;
        if (apps == null) apps = new List<SmartWatchApp>();
        if (apps.Count != SmartWatch.instance.apps.Count) apps = new List<SmartWatchApp>( SmartWatch.instance.apps.Count);
        
        for (int i = 0; i < SmartWatch.instance.apps.Count; i++)
            {
                if (apps.Count <= i)
                {
                    apps.Add(new SmartWatchApp(SmartWatch.instance.apps[i], this));
                }
                else
                {
                    apps[i].SetApp(SmartWatch.instance.apps[i], this);
                }
            } 
    }

    private void OnAppOpen(SmartWatch.App app)
    {
        foreach (var a in apps)
        {
            if (a.app == app) {
            
                if (ShowUnityEvent) a.onAppOpen.Invoke();
                if (ShowAnimatorTrigger) animator.SetTrigger(a.animatorTrigger);
            }
            
            else if (a.app.name == app.name)
            {
                if (ShowUnityEvent) a.onAppOpen.Invoke();
                if (ShowAnimatorTrigger) animator.SetTrigger(a.animatorTrigger);
            }
        }
    }
}
