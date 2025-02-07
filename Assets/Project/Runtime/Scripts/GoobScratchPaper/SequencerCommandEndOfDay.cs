using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;

public class SequencerCommandEndOfDay : SequencerCommand
{
    public void Awake()
    {
        GameManager.instance.EndOfDay();
        
        DialogueManager.instance.PlaySequence("HideCustomPanel(SmartWatch)");
        
    }
}
