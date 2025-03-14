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
        
        var allStartMenuPanels = FindObjectsByType< StartMenuPanel>( FindObjectsSortMode.None);
        
        foreach (var panel in allStartMenuPanels)
        {
            if (panel != this)
            {
                panel.Close();
            }
        }
        foreach (var selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.navigation = new UnityEngine.UI.Navigation()
            {
                mode = UnityEngine.UI.Navigation.Mode.Automatic
            };
        }
        
        GameManager.instance.OverrideDefaultSelectable( GetComponentInChildren<Button>());
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
