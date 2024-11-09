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
                AudioEngine.Instance.SetClipVolume(address, Mathf.Lerp(originalVolume, desiredVolume, t));
                yield return null;
                elapsed += DialogueTime.deltaTime;
            }
        }
        
    }

/// <summary>
/// Fades the volume of a channel in and out in the form of: ChannelFade(channel, direction|out, duration)
/// </summary>
    public class SequencerCommandChannelFade : SequencerCommand
    {
        private IEnumerator Start()
        {
            string address = GetParameter(0);
            
            string direction = GetParameter(1, "out");
            
            var split = direction.Split('|');
            
            string outcome = string.Empty;
            
            if (split.Length <= 1) outcome = direction == "out" ? "unstay" : "stay";
            
            if (split.Length > 1)
            {
                direction = split[0];
                outcome = split[1];
            }
            
            float duration = GetParameterAsFloat(2);
            
            var desiredVolume = direction switch
            {
                "in" => 1,
                "out" => 0,
                _ => 0
            };

            var originalVolume = AudioEngine.Instance.GetChannelVolume(address);

            
            if (duration == 0)
            {
                AudioEngine.Instance.SetChannelVolume(address, desiredVolume);
            }

          
            float elapsed = 0;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                AudioEngine.Instance.SetChannelVolume(address, Mathf.Lerp(originalVolume, desiredVolume, t));
                yield return null;
                elapsed += DialogueTime.deltaTime;
            }

            if (outcome is "unstay")
            {
                AudioEngine.Instance.SetChannelVolume(address, originalVolume);
                if (direction is "out") AudioEngine.Instance.StopAllAudioOnChannel(address);
            }
        }
    }
}