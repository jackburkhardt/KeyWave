using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.DialogueSystem
{
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
}