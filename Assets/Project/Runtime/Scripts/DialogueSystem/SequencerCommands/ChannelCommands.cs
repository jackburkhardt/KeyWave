using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;

namespace Project.Runtime.Scripts.DialogueSystem.SequencerCommands
{
    public class SequencerCommandSetChannelVolume : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            var volume = GetParameterAsFloat(1);
            AudioEngine.Instance.SetChannelVolume(channel, volume);
            Stop();
        }
    }
    
    public class SequencerCommandMuteChannel : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            AudioEngine.Instance.MuteChannel(channel);
            Stop();
        }
    }
    
    public class SequencerCommandUnmuteChannel : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            AudioEngine.Instance.UnmuteChannel(channel);
            Stop();
        }
    }
    
    public class SequencerCommandStopAllAudioOnChannel : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            AudioEngine.Instance.StopAllAudioOnChannel(channel);
            Stop();
        }
    }
    
    public class SequencerCommandPauseAllAudioOnChannel : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            AudioEngine.Instance.PauseAllAudioOnChannel(channel);
            Stop();
        }
    }
    
    public class SequencerCommandResumeAllAudioOnChannel : SequencerCommand
    {
        private void Awake()
        {
            var channel = GetParameter(0);
            AudioEngine.Instance.ResumeAllAudioOnChannel(channel);
            Stop();
        }
    }
}