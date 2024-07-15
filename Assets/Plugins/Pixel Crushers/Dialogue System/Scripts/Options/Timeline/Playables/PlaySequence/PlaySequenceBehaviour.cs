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
    public class PlaySequenceBehaviour : PlayableBehaviour
    {

        [Tooltip("Play this sequence.")]
        [TextArea(5, 5)]
        public string sequence;

        [Tooltip("(Optional) The other subject in the sequence.")]
        public Transform listener;

    }
}
#endif
#endif
