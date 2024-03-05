using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IconOnMap : MonoBehaviour

{
    [SerializeField] private MapAnimator _mapAnimator;
    [SerializeField] private string description;
    [SerializeReference] private Location _location;
    
    // Start is called before the first frame update
    void Start()
    {
        _mapAnimator??=GetComponentInParent<MapAnimator>();
    }

   

    public void OnHover()
    {
        /*
        _mapAnimator.ShowInfoPanelHandler(transform, _location, description,
            DistanceToPlayerLocation() * GameManager.TimeScales.GlobalTimeScale);
            */
    }

    public void OnMouseLeave()
    {
        /*
        _mapAnimator.HideInfoPanelHandler();
        */
    }
    
    
    public void OnClick()
    {
        /*
        _mapAnimator.ShowInfoPanelHandler(transform, _location, description,
            DistanceToPlayerLocation() * GameManager.TimeScales.GlobalTimeScale, true);
       _mapAnimator.ShowConfirmationButtons();
       _mapAnimator.ZoomInOnIcon(transform);
       */
    }

   
}
