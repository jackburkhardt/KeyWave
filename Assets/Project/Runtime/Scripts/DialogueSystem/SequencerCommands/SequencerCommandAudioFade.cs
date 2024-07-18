using System.Collections;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;
using UnityEngine;

namespace Project.Runtime.Scripts.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Syntax: AudioFade(audioSource, duration)
    /// </summary>
    public class SequencerCommandClipFade : SequencerCommand
    {
        private IEnumerator Start()
        {
            string address = GetParameter(0);
            float desiredVolume = GetParameterAsFloat(1);
            float duration = GetParameterAsFloat(2);
            if (duration == 0)
            {
                AudioEngine.Instance.SetClipVolume(address, desiredVolume);
            }
            
            float originalVolume = AudioEngine.Instance.GetClipVolume(address);
            float elapsed = 0;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                AudioEngine.Instance.SetClipVolume(address, Mathf.Lerp(originalVolume, 0, t));
                yield return null;
                elapsed += DialogueTime.deltaTime;
            }
        }
        
    }


    public class SequencerCommandChannelFade : SequencerCommand
    {
        private IEnumerator Start()
        {
            string address = GetParameter(0);
            float desiredVolume = GetParameterAsFloat(1);
            float duration = GetParameterAsFloat(2);
            if (duration == 0)
            {
                AudioEngine.Instance.SetChannelVolume(address, desiredVolume);
            }

            float originalVolume = AudioEngine.Instance.GetChannelVolume(address);
            float elapsed = 0;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                AudioEngine.Instance.SetChannelVolume(address, Mathf.Lerp(originalVolume, 0, t));
                yield return null;
                elapsed += DialogueTime.deltaTime;
            }
        }
    }
}