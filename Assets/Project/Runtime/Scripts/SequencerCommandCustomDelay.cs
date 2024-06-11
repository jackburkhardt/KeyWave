using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;

namespace Project.Runtime.Scripts
{

    public class SequencerCommandCustomDelay : SequencerCommand
    {
        private string defaultSequence = "WaitForMessage(Typed); Delay({{end}});";

        private float stopTime;

        public void Start()
        {
            float seconds = GetParameterAsFloat(0);
            stopTime = DialogueTime.time + seconds;
            // if (DialogueDebug.logInfo) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: Sequencer: Delay({1})", new System.Object[] { DialogueDebug.Prefix, seconds }));
        }

        public void Update()
        {
            if (DialogueTime.time >= stopTime) Stop();
        }
    }

}