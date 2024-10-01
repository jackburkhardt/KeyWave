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

    protected override int ActiveIndex => Mathf.Max(ComponentsToSwitch.FindIndex(c => c.activeSelf), 0);

    protected override List<GameObject> ComponentsToSwitch => Target.GetComponentsInChildren<Transform>(true).ToList()
        .FindAll(t => t.gameObject != Target.gameObject && (t.parent == Target.transform || !rootsOnly))
        .Select(t => t.gameObject).ToList();

    public override void ShowComponent(GameObject obj)
    {
        obj.SetActive(true);
    }

    public override void HideComponent(GameObject obj)
    {
       obj.SetActive(false);
    }
}
