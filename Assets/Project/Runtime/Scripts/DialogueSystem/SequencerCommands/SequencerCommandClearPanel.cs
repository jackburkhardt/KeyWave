using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;

namespace Project.Runtime.Scripts.DialogueSystem
{

    public class SequencerCommandClearPanel : SequencerCommand
    {
        public void Awake()
        {
            var subtitleManager = FindObjectOfType<SubtitleManager>();
            subtitleManager.ClearContents();


            Stop();
        }
    }

}