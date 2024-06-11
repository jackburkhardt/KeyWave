using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public class CustomSequencerShortcuts : MonoBehaviour
    {
        private void Awake()
        {
            Sequencer.RegisterShortcut("fade", "HidePanel(0); Fade(stay, 0.5)@0.5; Fade(unstay, 0.5)@1.5; Delay(3)");
        
            Sequencer.RegisterShortcut("typed", "Continue()@Message(Typed)");
        
        }
    }
}