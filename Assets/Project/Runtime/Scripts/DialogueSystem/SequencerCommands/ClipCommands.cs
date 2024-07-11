using System;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;

namespace Project.Runtime.Scripts.DialogueSystem.SequencerCommands
{
    public class SequencerCommandPlayClip : SequencerCommand
    {
        private void Awake()
        {
            var address = GetParameter(0);
            var repeats = GetParameterAsInt(1, 0);
            
            if (repeats == 0)
            {
                AudioEngine.Instance.PlayClip(address);
            }
            else
            {
                AudioEngine.Instance.PlayClip(address, repeats);
            }
            Stop();
        }
    }
    
    public class SequencerCommandPlayClipLooped : SequencerCommand
    {
        private void Awake()
        {
            var address = GetParameter(0);
            AudioEngine.Instance.PlayClipLooped(address);
            Stop();
        }
    }
    
    public class SequencerCommandStopClip : SequencerCommand
    {
        private void Awake()
        {
            var address = GetParameter(0);
            AudioEngine.Instance.StopClip(address);
            Stop();
        }
    }
    
    public class SequencerCommandStopAllAudio : SequencerCommand
    {
        private void Awake()
        {
            AudioEngine.Instance.StopAllAudio();
            Stop();
        }
    }
    
    public class SequencerCommandPauseAllAudio : SequencerCommand
    {
        private void Awake()
        {
            AudioEngine.Instance.PauseAllAudio();
            Stop();
        }
    }
    
    public class SequencerCommandResumeAllAudio : SequencerCommand
    {
        private void Awake()
        {
            AudioEngine.Instance.ResumeAllAudio();
            Stop();
        }
    }
}