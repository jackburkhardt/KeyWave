using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class DialogueSystemCustomMethods : MonoBehaviour
{
    public void PauseForSeconds(float seconds)
    {
        StartCoroutine(PauseForSecondsCoroutine(seconds));
    }
    
    private IEnumerator PauseForSecondsCoroutine(float seconds)
    {
        DialogueManager.Pause();
        yield return new WaitForSeconds(seconds);
        DialogueManager.Unpause();
    }
}
