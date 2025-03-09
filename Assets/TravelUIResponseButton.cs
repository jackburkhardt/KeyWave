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

public class TravelUIResponseButton : StandardUIResponseButton
{
    public UITextField ETALabel;
    public UITextField description;
    
    private Location location;
    
    public static Action<Location> OnLocationSelected;
    public UnityEvent onLocationSelected;

    public Button confirmButton;
    public Graphic confirmButtonGraphic;
    public UITextField confirmButtonText;
    
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

            transform.localPosition = location.Name == "Café" ? GameManager.gameState.GetPlayerLocation().LookupVector2("Coordinates") : location.LookupVector2("Coordinates");
            
            confirmButton.interactable = true;

            if (location.FieldExists("Open Time"))
            {
                var openTime = location.LookupInt("Open Time");
                var closeTime = location.LookupInt("Close Time");
                var rawETA = Clock.EstimatedTimeOfArrivalRaw(location.id);
                
                if (rawETA < openTime || rawETA > closeTime)
                {
                    confirmButton.interactable = false;
                    confirmButtonGraphic.color = Color.red;
                    confirmButtonText.text = "Closed on Arrival";
                }
            }
            
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
