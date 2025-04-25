using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPanel : UIPanel
{
    public override void Open()
    {
        base.Open();
        
        RefreshLayoutGroups.Refresh( gameObject);
        
        
        // ensure that only one StartMenuPanel is open at a time
        
        var allStartMenuPanels = FindObjectsByType< StartMenuPanel>( FindObjectsSortMode.None);
        
        foreach (var panel in allStartMenuPanels)
        {
            if (panel != this)
            {
                panel.Close();
            }
        }
        
        // set the navigation mode of all selectables to automatic to ensure proper keyboard navigation
        // otherwise the keyboard can get stuck on non-visible selectables
        
        
        foreach (var selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.navigation = new UnityEngine.UI.Navigation()
            {
                mode = UnityEngine.UI.Navigation.Mode.Automatic
            };
        }
        
        InputManager.instance.OverrideDefaultSelectable( GetComponentInChildren<Button>());
    }
    
    
    public override void Close()
    {
        base.Close();

        foreach (var selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.navigation = new UnityEngine.UI.Navigation()
            {
                mode = UnityEngine.UI.Navigation.Mode.None
            };
        }
    }
    
    
    
}
