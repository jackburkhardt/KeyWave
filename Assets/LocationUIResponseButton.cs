using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationUIResponseButton : StandardUIResponseButton
{
    public UITextField ETALabel;
    public UITextField description;
    
    private Location location;
    
    public static Action<Location> OnLocationSelected;
    public UnityEvent onLocationSelected;
    
    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
           
            var locationField = response.destinationEntry.fields.Find( p => p.title == "Location");
            location = DialogueManager.masterDatabase.GetLocation(int.Parse(locationField.value));

            description.text = location.Description;
            ETALabel.text = $"{Clock.EstimatedTimeOfArrival(location.id)}";
            GetComponent<Image>().color = location.LookupColor("Color");
            transform.localPosition = location.LookupVector2("Coordinates");

        }
    }

    public override void OnClick()
    {
        base.OnClick();
        GameManager.instance.SetLocation( location.Name);
    }
    
    public override void OnSelect( BaseEventData data)
    {
        base.OnSelect(data);
        OnLocationSelected?.Invoke(location);
        onLocationSelected?.Invoke();
    }
}
