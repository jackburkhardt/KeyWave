// Recompile at 3/18/2024 10:31:56 AM

#if USE_TIMELINE
#if UNITY_2017_1_OR_NEWER
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;
using System;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class ShowAlertBehaviour : PlayableBehaviour
    {

        [Tooltip("Show this message using the Dialogue System's alert panel.")]
        public string message;

        [Tooltip("Show alert for duration based on text length, not duration of playable clip.")]
        public bool useTextLengthForDuration;

    }
}
#endif
#endif
