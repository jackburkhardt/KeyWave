
/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomBlackboardRequirements : EventRequirementsCheck
{
    [Space]
    public blackboard.RoomBlackboard room;
    public string previousPlayerVisitsRequired;
    public bool isPlayerHereRequired = true;

    protected override void CheckRequirements()
    {
        base.CheckRequirements();
        if (!requirementsMatched) return;


        if (isPlayerHereRequired && !room.isPlayerHere) {
            requirementsMatched = false;
            requirementsDebugLog += "isPlayerHere ";
            return;
        }
        if (!EvaluateStringInequality(previousPlayerVisitsRequired, room.previousTotalPlayerVisits))
        {
            requirementsMatched = false;
            requirementsDebugLog += "previousPlayerVisits ";
            return;
        }
    }

}

*/
