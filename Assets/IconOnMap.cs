using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IconOnMap : MonoBehaviour

{
    [SerializeField] private MapAnimator _mapAnimator;
    [SerializeField] private string description;
    [SerializeReference] private GameManager.Locations _location;
    
    // Start is called before the first frame update
    void Start()
    {
        _mapAnimator??=GetComponentInParent<MapAnimator>();
    }

    private int DistanceToPlayerLocation()
    {
        var allMapPins = FindObjectsOfType<IconOnMap>();

        var distance = 0;
        
        foreach (var mapPin in allMapPins)
        {
            if (mapPin._location.ToString() == GameStateManager.instance.gameState.player_location)
            {
                distance = (int)Vector2.Distance(mapPin.transform.localPosition, transform.localPosition);
            }
        }
        
    

        return distance;

    }

    public void OnHover() => _mapAnimator.ShowInfoPanel(transform, _location.ToString(), description, DistanceToPlayerLocation() * GameManager.TimeScales.GlobalTimeScale);

    public void OnMouseLeave()
    {
        _mapAnimator.HideInfoPanelHandler();
    }
    
    
    public void OnClick()
    {
       _mapAnimator.ShowConfirmationButtons();
    }

   
}
