using System.Linq;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;

namespace Project.Runtime.Scripts.DialogueSystem
{

    public class SequencerCommandClearPanel : SequencerCommand
    {
        public void Awake()
        {
            var subtitleManager = FindObjectsOfType<SubtitleManager>().Where(x => x.gameObject.activeSelf);
            foreach (var manager in subtitleManager)
            {
                manager.ClearContents();
            }
            
            Stop();
        }
    }

}