using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.UI;
using UnityEngine;

public class LocationPanel : UIPanel
{
    public UITextField locationName;
    public UITextField locationDescription;
    public UITextField specialDescription;
    
    public RectTransform actorStatusContainer;
    public DialogueActorInfo actorStatusTemplate;
    
    public MetricsGrid metricsGrid;
    
    private Animator _animator;
    public RectTransform noCharactersPresent;

    private void Awake()
    {
        _animator ??= GetComponent<Animator>();
    }

    private void OnValidate()
    {
        _animator ??= GetComponent<Animator>();
    }

    public void SetLocationInfo(Location location)
    {
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

    public void SetLocationInfo(CustomUIResponseButton customUIResponseButton)
    {
        var location = customUIResponseButton.assignedLocation;
        
        if (location == null)
        {
            Debug.LogError("Location not assigned to CustomUIResponseButton");
            return;
        }
        
        SetLocationInfo(location);
    }
    
    public void ShowLocationInfo(Location location)
    {
        if (isOpen)
        {
            StartCoroutine(ShowLocation());
        }

        else
        {
            Open();
            SetLocationInfo(location);
        }
        
        IEnumerator ShowLocation()
        {
            Close();
            yield return new WaitForEndOfFrame();
            var clip = _animator.GetCurrentAnimatorClipInfo(0);
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            var transition = _animator.GetAnimatorTransitionInfo(0);
            
            var longestClip = clip.Max(c => c.clip.length) * state.speed;
            var animationLength = Mathf.Max(longestClip, transition.duration);
            yield return new WaitForSeconds(animationLength);
            
            ShowLocationInfo(location);
        }
    }
    
    public void ShowLocationInfo(CustomUIResponseButton customUIResponseButton)
    {
        var locationField = customUIResponseButton.response.destinationEntry.fields.First(p => p.title == "Location");
        
        
        
        if (locationField == null)
        {
            Debug.LogError("Location not assigned to CustomUIResponseButton");
            return;
        }
        
        var location = DialogueManager.masterDatabase.GetLocation(int.Parse(locationField.value));
        
        ShowLocationInfo(location);
    }
    
 
}
