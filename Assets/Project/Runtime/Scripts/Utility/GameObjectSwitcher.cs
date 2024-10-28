using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameObjectSwitcher : ComponentSwitcher<GameObject>
{

    public override int ActiveIndex => Mathf.Max(ComponentsToSwitch.FindIndex(c => c.activeSelf), 0);

    protected override List<GameObject> ComponentsToSwitch => Target.GetComponentsInChildren<Transform>(true).ToList()
        .FindAll(t => t.gameObject != Target.gameObject && (t.parent == Target.transform || !rootsOnly))
        .Select(t => t.gameObject).ToList();

    public override void ShowComponent(GameObject obj)
    {
        obj.SetActive(true);
        if (Application.isPlaying && broadcastMessage)
        {
            BroadcastMessage(ShowComponentMessage, SendMessageOptions.DontRequireReceiver);
            
        }
        //else  obj.SetActive(true);
       
    }

    public override void HideComponent(GameObject obj)
    {
        obj.SetActive(false);
        if (Application.isPlaying && broadcastMessage)
        {
            BroadcastMessage(HideComponentMessage, SendMessageOptions.DontRequireReceiver);
            
        }
    }

    public void OnEnable()
    {
        SwitchTo(ActiveIndex);
    }

    public void OnDisable()
    {
        HideAll();
    }

    [Label("Try Broadcast Message First")]
    [ShowIf("ShowExtras")] public bool broadcastMessageOnRuntimeInstead;
    
    private bool broadcastMessage => broadcastMessageOnRuntimeInstead == true && ShowExtras;

    [ShowIf("broadcastMessage")] public string ShowComponentMessage = "Open";
    
    [ShowIf("broadcastMessage")] public string HideComponentMessage = "Close";
}
