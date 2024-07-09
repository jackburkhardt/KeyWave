using System.Collections;
using UnityEngine;
namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Syntax: AudioFade(audioSource, duration)
    /// </summary>
    public class SequencerCommandAudioFade : SequencerCommand
    {
        private AudioSource audioSource;
        private float originalVolume;

        private IEnumerator Start()
        {
            Transform subject = GetSubject(0, speaker);
            if (subject == null) yield break;

            audioSource = subject.GetComponent<AudioSource>();
            if (audioSource == null) yield break;
            originalVolume = audioSource.volume;

            float duration = GetParameterAsFloat(1);
            if (duration == 0) yield break;

            float elapsed = 0;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                audioSource.volume = Mathf.Lerp(originalVolume, 0, t);
                yield return null;
                elapsed += DialogueTime.deltaTime;
            }
        }

        private void OnDestroy()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.volume = originalVolume;
            }
        }
    }
}