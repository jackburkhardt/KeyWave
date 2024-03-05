using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DelayedUIResponseButton : StandardUIResponseButton
{
    public int delayTime;

    private bool isPaused = false;

    public override void OnClick()
    {
        StartCoroutine(DelayThenClick());
    }
    IEnumerator DelayThenClick()
    {
        yield return new WaitForSeconds(delayTime);
        base.OnClick();
    }
}
