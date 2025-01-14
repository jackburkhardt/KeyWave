using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutuallyExclusiveSelectables : MonoBehaviour
{
    public List<Selectable> selectables;
    
    public void MakeInteractable(Selectable selected)
    {
        foreach (var selectable in selectables)
        {
            selectable.interactable = selected == selectable;
        }
    }
    
    public void MakeAllInteractable()
    {
        foreach (var selectable in selectables)
        {
            selectable.interactable = true;
        }
    }
}
