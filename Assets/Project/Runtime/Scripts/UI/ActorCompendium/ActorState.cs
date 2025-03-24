// Copyright (c) Pixel Crushers. All rights reserved.

using System;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// Quest state is a bit-flag enum that indicates the state of a quest. 
    /// This enum is used by the QuestLog class.
    /// </summary>
    [Flags]
    public enum ActorState
    {

        /// <summary>
        /// Quest is unassigned
        /// </summary>
        Unidentified = 0x1,

        /// <summary>
        /// Quest is active (assigned but not completed yet)
        /// </summary>
        Mentioned = 0x2,

        /// <summary>
        /// Quest was completed successfully; corresponds to "success" or "done"
        /// </summary>
        Amicable = 0x4,

        /// <summary>
        /// Quest was completed in failure
        /// </summary>
        Botched = 0x8,

        /// <summary>
        /// Quest was abandoned
        /// </summary>
        Unapproachable = 0x10,

        /// <summary>
        /// Quest is available to be granted to the player. The Dialogue System does
        /// not use this state, but it's included for those who want to use it on their own
        /// </summary>
        Approachable = 0x20,

        /// <summary>
        /// Quest is waiting for player to return to NPC. The Dialogue System does
        /// not use this state, but it's included for those who want to use it on their own
        /// </summary>
        Confronted = 0x40
    }

}