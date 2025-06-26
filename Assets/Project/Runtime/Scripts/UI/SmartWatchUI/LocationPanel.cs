using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Sentry.Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using App = Project.Runtime.Scripts.App.App;

/// <summary>
/// This panel displays information about a location when clicking on it in the travel SmartWatch app.
/// </summary>

public class LocationPanel : UIPanel
{
    [SmartWatchAppPopup] public string app;

    public Graphic panel;
    public Color defaultColor;
    [Label("Location Unavailable Color")]
    public Color closedColor;
    
    
    public UITextField locationName;
    public UITextField locationHours;
    public UITextField locationDescription;
    public UITextField specialDescription;
    
    [FormerlySerializedAs("actorStatusContainer")] public RectTransform actionStatusContainer;
    [FormerlySerializedAs("actorStatusTemplate")] public DialogueActorInfo actionStatusTemplate;
    
    public MetricsGrid metricsGrid;
    
    private Animator _animator;
    public RectTransform noCharactersPresent;
    
    private Location _location;

    public static Action onLocationPanelClose;

    private void Awake()
    {
        _animator ??= GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        _animator ??= GetComponent<Animator>();
        App.OnSceneDeloadStart += OnSceneDeloadStart;
        base.OnEnable();
        
    }

    protected override void OnDisable()
    {
        App.OnSceneDeloadStart -= OnSceneDeloadStart;
        base.OnDisable();
    }

    private void OnValidate()
    {
        _animator ??= GetComponent<Animator>();
    }

    public void OnGameSceneStart()
    {
        // Since this function is called even if the game object is disabled, we subscribe to events here instead of OnEnable
        SmartWatchPanel.onAppOpen += OnAppOpen;
        TravelUIResponseButton.OnLocationSelected += ShowLocationInfo;
        if (gameObject.activeSelf) Close();
    }



    public void OnGameSceneEnd()
    {
        SmartWatchPanel.onAppOpen -= OnAppOpen;
        TravelUIResponseButton.OnLocationSelected -= ShowLocationInfo;
        Close();


    }
    
    public void OnSceneDeloadStart(string scene)
    {
        // Sometimes the panel is not closed when the game scene ends, so this is a contingency to ensure it is closed.
        Close();
    }

    public void SetLocationInfo(Location location)
    {
        _location = location;
        locationName.text = location.IsFieldAssigned("Display Name") ? location.AssignedField("Display Name").value: location.Name;
        
        var locationHasHours = location.IsFieldAssigned("Open Time") && location.IsFieldAssigned("Close Time");
        var locationIsOpen = !locationHasHours || Clock.EstimatedTimeOfArrivalRaw(location) >= location.LookupInt("Open Time") && Clock.EstimatedTimeOfArrivalRaw(location) <= location.LookupInt("Close Time");

        locationHours.gameObject.SetActive(locationHasHours);

        if (locationHasHours)
        {
            var openTime = Clock.To24HourClock( location.LookupInt("Open Time"));
            var closeTime = Clock.To24HourClock( location.LookupInt("Close Time"));
            locationHours.text = $"Hours:  <b>{openTime} - {closeTime}</b>";
            
            if (!locationIsOpen)
            {
                locationHours.text += " (Closed)";
                locationHours.color = Color.red;
                panel.color = closedColor;
            }
            
            else
            {
                locationHours.text += " (Open)";
                locationHours.color = Color.green;
                panel.color = defaultColor;
            }
        }

        else
        {
            panel.color = defaultColor;
        }
        
        
        locationDescription.text = "Some things you can do here:";
        specialDescription.text = location.IsFieldAssigned("Special Description") ? location.AssignedField("Special Description").value : "";
        
        foreach (Transform child in actionStatusContainer)
        {
            if (child == actionStatusTemplate.transform || child == noCharactersPresent.transform) continue;
            Destroy(child.gameObject);
        }
        
        actionStatusTemplate.gameObject.SetActive(false);
        noCharactersPresent.gameObject.SetActive(true);

        foreach (var action in DialogueManager.instance.masterDatabase.items)
        {
            if (!action.IsAction) continue;
            if (int.Parse(action.GetField("Location").value) != location.id) continue;
            var actionName = DialogueUtility.GetConditionalDisplayName(action);
            var actorStatus = Instantiate(actionStatusTemplate, actionStatusContainer);
            actorStatus.gameObject.SetActive(true);
            actorStatus.SetActionInfo(actionName);
            noCharactersPresent.gameObject.SetActive(false);
        }
        
        metricsGrid.EnableValidMetrics(MetricsGrid.DisplayCondition.HighLocationAffinity, location);
    }

    public void ShowLocationInfo(StandardUIResponseButton standardUIResponseButton)
    {
        
        var dialogueEntry = standardUIResponseButton.response.destinationEntry;
        var locationField = dialogueEntry.fields.First(p => p.title == "Location");
        var location = DialogueManager.masterDatabase.GetLocation(int.Parse(locationField.value));
        
        if (!isOpen) Open();
        ShowLocationInfo(location);
    }
    
    public void OnAppOpen(SmartWatchAppPanel appPanel)
    {
        if (appPanel.Name != app)
        {
            Close(); return;
        }
        
        Open();
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        if (_location == null) return;
        base.Open();
    }

    public override void Close()
    {
        _location = null;
        if (isOpen) onLocationPanelClose?.Invoke();
        base.Close();
      
    }
    
    public void ShowLocationInfo(Location location)
    {
        
        if (_location != null)
        {
            StartCoroutine(CloseThenShowLocation());
        }

        else
        {
            SetLocationInfo(location);
            Open();
        }
        
        IEnumerator CloseThenShowLocation()
        {
            _animator.SetTrigger(hideAnimationTrigger);
            yield return new WaitForEndOfFrame();
            var clip = _animator.GetCurrentAnimatorClipInfo(0);
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            var transition = _animator.GetAnimatorTransitionInfo(0);
            
            var longestClip = clip.Max(c => c.clip.length) * state.speed;
            var animationLength = Mathf.Max(longestClip, transition.duration);
            yield return new WaitForSeconds(animationLength);
            SetLocationInfo(location);
            _animator.SetTrigger(showAnimationTrigger);
           
        }
    }
    
 
}
