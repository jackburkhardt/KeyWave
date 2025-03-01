using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ActionUIResponseButton : StandardUIResponseButton
{
    public UITextField description;

    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
        }
    }
}
