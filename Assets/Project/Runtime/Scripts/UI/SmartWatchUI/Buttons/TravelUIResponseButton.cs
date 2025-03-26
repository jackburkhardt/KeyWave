using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TravelUIResponseButton : StandardUIResponseButton, IDeselectHandler
{
    public UITextField ETALabel;
    public UITextField description;
    public static Action<Location> OnLocationSelected;
    public UnityEvent onLocationSelected;

    public Button confirmButton;
    public Graphic confirmButtonGraphic;
    public UITextField confirmButtonText;
    
    private Location _location;
    private bool _isSelected;
    private InputAction submitAction;

    public override void Start()
    {
        base.Start();
        submitAction = FindObjectOfType<InputSystemUIInputModule>().submit;
    }
    
    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
           
            var locationField = response.destinationEntry.fields.Find( p => p.title == "Location");
            _location = DialogueManager.masterDatabase.GetLocation(int.Parse(locationField.value));

            description.text = _location.Description;
            ETALabel.text = $"{Clock.EstimatedTimeOfArrival(_location)}";
            GetComponent<Image>().color = _location.LookupColor("Color");

            transform.localPosition = _location.Name == "Café" ? GameManager.instance.locationManager.PlayerLocation.LookupVector2("Coordinates") : _location.LookupVector2("Coordinates");
            
            confirmButton.interactable = true;

            if (_location.FieldExists("Open Time"))
            {
                var openTime = _location.LookupInt("Open Time");
                var closeTime = _location.LookupInt("Close Time");
                var rawETA = Clock.EstimatedTimeOfArrivalRaw(_location);
                
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
        LocationManager.SetPlayerLocation( _location);
    }

    
    
    public override void OnSelect( BaseEventData data)
    {
        base.OnSelect(data);
        OnLocationSelected?.Invoke(_location);
        onLocationSelected?.Invoke();
        _isSelected = true;
    }

    protected virtual void Update()
    {
        if (_isSelected && (submitAction.WasPressedThisFrame() && gameObject.activeSelf) && confirmButton.interactable)
        {
            confirmButton.onClick.Invoke();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
    }
}
