using System.Collections;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class LocationPanel : UIPanel
{
    [SmartWatchAppPopup] public string app;
    
    public UITextField locationName;
    public UITextField locationDescription;
    public UITextField specialDescription;
    
    public RectTransform actorStatusContainer;
    public DialogueActorInfo actorStatusTemplate;
    
    public MetricsGrid metricsGrid;
    
    private Animator _animator;
    public RectTransform noCharactersPresent;
    
    private Location _location;

    private void Awake()
    {
        _animator ??= GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LocationUIResponseButton.OnLocationSelected += ShowLocationInfo;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LocationUIResponseButton.OnLocationSelected -= ShowLocationInfo;
    }

    private void OnValidate()
    {
        _animator ??= GetComponent<Animator>();
    }

    public void OnGameSceneStart()
    {
        Debug.Log( "Location Panel is starting");
        SmartWatchPanel.onAppOpen += OnAppOpen;
    }

    public void OnGameSceneEnd()
    {
        SmartWatchPanel.onAppOpen -= OnAppOpen;
        Close();
    }

    public void SetLocationInfo(Location location)
    {
        _location = location;
        locationName.text = location.IsFieldAssigned("Display Name") ? location.AssignedField("Display Name").value: location.Name;
        locationDescription.text = location.Description;
        specialDescription.text = location.IsFieldAssigned("Special Description") ? location.AssignedField("Special Description").value : "";
        
        foreach (Transform child in actorStatusContainer)
        {
            if (child == actorStatusTemplate.transform || child == noCharactersPresent.transform) continue;
            Destroy(child.gameObject);
        }
        
        actorStatusTemplate.gameObject.SetActive(false);
        noCharactersPresent.gameObject.SetActive(true);
        
        foreach (var actor in DialogueManager.instance.masterDatabase.actors)
        {
            if (!actor.IsFieldAssigned("Location") || actor.AssignedField("Location").value != location.id.ToString()) continue;
            var actorStatus = Instantiate(actorStatusTemplate, actorStatusContainer);
            actorStatus.gameObject.SetActive(true);
            actorStatus.SetActorInfo(actor, "is present.");
            noCharactersPresent.gameObject.SetActive(false);
        }
        
        metricsGrid.EnableValidMetrics(MetricsGrid.DisplayCondition.HighLocationAffinity, location);
        
    }

    public void ShowLocationInfo(StandardUIResponseButton standardUIResponseButton)
    {
       
        var dialogueEntry = standardUIResponseButton.response.destinationEntry;
        var locationField = dialogueEntry.fields.First(p => p.title == "Location");
        var location = DialogueManager.masterDatabase.GetLocation(int.Parse(locationField.value));
        
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
