using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwitcher : ComponentSwitcher<Color>
{

    public Graphic targetGraphic;
    public List<Color> colors;
    public override int ActiveIndex => Mathf.Max(colors.FindIndex(p => targetGraphic.color == p), 0);
    protected override List<Color> ComponentsToSwitch => colors;
    public override void ShowComponent(Color color)
    {
        targetGraphic.color = ComponentsToSwitch[Mathf.Max(colors.FindIndex(p => color == p), 0)];
    }

    protected override bool ShowTargetInInspector => false;

    public override void HideComponent(Color obj)
    {
        targetGraphic.color = Color.clear;
    }
    
    private void OnValidate()
    {
        if (sync)
        {
            if (colors.Count > SyncedComponentCount)
            {
                colors.RemoveRange(SyncedComponentCount, colors.Count - SyncedComponentCount);
            }
            else if (colors.Count < SyncedComponentCount)
            {
                colors.AddRange(new Color[SyncedComponentCount - colors.Count]);
            }
        }
    }
}
