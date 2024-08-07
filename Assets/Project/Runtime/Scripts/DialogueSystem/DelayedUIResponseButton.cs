using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public class DelayedUIResponseButton : StandardUIResponseButton
    {
        public int buttonDelayTime;
        private bool isPaused = false;

        public override void OnClick()
        {
            StartCoroutine(DelayThenClick());
        }

        IEnumerator DelayThenClick()
        {
            yield return new WaitForSeconds(buttonDelayTime);
            base.OnClick();
        }
    }
}