
using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
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

