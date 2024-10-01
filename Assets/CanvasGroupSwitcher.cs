using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasGroupSwitcher : ComponentSwitcher<CanvasGroup>
{
    protected override int ActiveIndex => ComponentsToSwitch.FindIndex(x => (int)x.alpha == 1);
    protected override List<CanvasGroup> ComponentsToSwitch => Target.GetComponentsInChildren<CanvasGroup>(true).Where(x => x.transform.parent == Target.transform).ToList(); 
    public override void ShowComponent(CanvasGroup obj)
    {
        obj.GetComponent<CanvasGroup>().alpha = 1;
    }

    public override void HideComponent(CanvasGroup obj)
    {
        obj.GetComponent<CanvasGroup>().alpha = 0;
    }
}
