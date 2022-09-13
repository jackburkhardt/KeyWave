using System;
using System.Collections.Generic;
using Assignments;
using Interaction;

public class Character
{
    public readonly string Name;
    private int Social;
    private int Technical;
    private int Creative;
    private int Trust;
    public List<Assignment> DelegatedAssignments;

    public bool TryReceiveAssignment(Assignment a)
    {
        if (!a.CanDelegate) return false;

        UpdateCompletionSpeed(a);
        
        DelegatedAssignments ??= new List<Assignment>();
        DelegatedAssignments.Add(a);
        
        GameEvent.OnTimeChange += UpdateAssignmentProgress;
    }

    private void UpdateCompletionSpeed(Assignment a)
    {
        float[][] modifierMatrix =
        {
            new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
            new float[] {2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f},
            new float[] {0f, 2.0f, 1.0f, 0.5f, 0.33f, 0.25f},
            new float[] {0f, 0f, 2.0f, 1.0f, 0.5f, 0.33f},
            new float[] {0f, 0f, 0f, 2.0f, 1.0f, 0.5f},
            new float[] {0f, 0f, 0f, 3.0f, 2.0f, 1.0f},
        };

        int cSkill = a.RequiredSkillName switch
        {
            "Social" => Social,
            "Technical" => Technical,
            "Creative" => Creative,
            _ => throw new Exception("ComputeCompletionSpeed: Unknown skill name")
        };

        float skillModifier = modifierMatrix[a.RequiredSkillLevel][cSkill];
        a.TimeToComplete *= skillModifier;
        
        float trustModifier = Trust switch
        {
            (<= 20 and >= 0) => 1.1f,
            (>= 50 and < 70) => 0.9f,
            (>= 70 and < 90) => 0.8f,
            (>= 90 and <= 100) => 0.7f,
            _ => 1
        };

        a.TimeToComplete *= trustModifier;
    }
    
    private void UpdateAssignmentProgress()
    {
        foreach (var assignment in DelegatedAssignments)
        {
            assignment.TimeToComplete -= 1;
            if (assignment.TimeToComplete <= TimeSpan.Zero)
            {
                assignment.Complete();
                DelegatedAssignments.Remove(assignment);
            }
        }
    }

}




}
