using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameObjective", menuName = "Objective")]
public class GameObjective : ScriptableObject
{

    public GameManager.Locations location;
    public string objectiveTitle;
    public Clock.Hour hour;
    public Clock.Minute minute;

    public enum State
    {
        unassigned,
        active,
        success,
        failure,
        done,
        abandoned,
        grantable
    }
    
    public State state;


}
