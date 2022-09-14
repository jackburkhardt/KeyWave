using System;
using System.Collections.Generic;
using Assignments;

public class Character
{
    public readonly string Name;
    private int social;
    private int technical;
    private int creative;
    private int trust;
    private Dictionary<Assignment, TimeSpan> delegatedAssignments;

    private const float MULTI_ASSIGNMENT_PENALTY = 1.1f;

    public Character(string name, int social, int technical, int creative, int trust, Dictionary<Assignment, TimeSpan> delegatedAssignments)
    {
        Name = name;
        this.social = social;
        this.technical = technical;
        this.creative = creative;
        this.trust = trust;
        this.delegatedAssignments = delegatedAssignments;
    }

    public void DelegateAssignment(Assignment a)
    {
        var modifier = CalculateCompletionModifier(a);

        if (delegatedAssignments == null)
        {
            delegatedAssignments = new Dictionary<Assignment, TimeSpan>();
            GameEvent.OnTimeChange += UpdateAssignmentProgress;
        }

        delegatedAssignments.Add(a, modifier * a.TimeToComplete);
        GameEvent.DelegateAssignment(a, this);
        // apply multiqueue penalty if more than one assignment is active
        if (delegatedAssignments.Count > 1) DoMultiAssignmentPenalty(true);
        
    }
    
    public void UndelegateAssignment(Assignment a)
    {
        if (delegatedAssignments == null || !delegatedAssignments.ContainsKey(a)) return;

        delegatedAssignments.Remove(a);
        // remove penalty if other assignments are also active
        if (delegatedAssignments.Count > 0) DoMultiAssignmentPenalty(false);
            
        if (delegatedAssignments.Count == 0) GameEvent.OnTimeChange -= UpdateAssignmentProgress;
    }

    /// <summary>
    /// Calculates the modifier for the assignment time based on character skills, assignment requirements, and
    /// trust value.
    /// </summary>
    /// <param name="a">The assignment to be modified.</param>
    /// <returns>A modifier to be multiplied by the assignment completion time. Will return 0 if assignment is not
    /// completable by this character.</returns>
    private float CalculateCompletionModifier(Assignment a)
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

        // this uses the required skill from the assignment and grabs this character's level for that skill
        int cSkill = a.RequiredSkillName switch
        {
            "Social" => social,
            "Technical" => technical,
            "Creative" => creative,
            _ => throw new Exception("ComputeCompletionSpeed: Unknown skill name")
        };
        // then it uses the above matrix to get the skill modifier
        float skillModifier = modifierMatrix[a.RequiredSkillLevel][cSkill];

        // this trust modifier is applied on top of the skill modifier. so if the trust is 60, the skillModifier is increased by 10%
        float trustModifier = trust switch
        {
            (<= 20 and >= 0) => 0.9f,
            (>= 50 and < 70) => 1.1f,
            (>= 70 and < 90) => 1.2f,
            (>= 90 and <= 100) => 1.3f,
            _ => 1
        };
        skillModifier *= trustModifier;
        
        return skillModifier;
    }
    
    private void UpdateAssignmentProgress(TimeSpan newTime)
    {
        foreach (var (updatedA, value) in delegatedAssignments)
        {
            var updatedT = value - TimeSpan.FromMinutes(RealtimeManager.TIMESTEP_GAMETIME_MINS);
            delegatedAssignments.Remove(updatedA);
            
            if (updatedT <= TimeSpan.Zero)
            {
                updatedA.Complete();
            }
            else
            {
                delegatedAssignments.Add(updatedA, updatedT);
            }
        }
    }

    /// <summary>
    /// Adds or removes a time penalty on all assignments. If this character is doing multiple assignments at once, a
    /// penalty is added, but as the amount they are doing decreases, the penalty is lowered. Penalties are compounded,
    /// so as more assignments are added, the penalty increases.
    /// </summary>
    /// <param name="apply">True to apply the penalty, false to remove it.</param>
    private void DoMultiAssignmentPenalty(bool apply)
    {
        // increase or decrease the time for each delegated assignment by 10%
        foreach (var (a, t) in delegatedAssignments)
        {
            delegatedAssignments.Remove(a);
            delegatedAssignments.Add(a, apply ? t * MULTI_ASSIGNMENT_PENALTY : t * 1 / MULTI_ASSIGNMENT_PENALTY);
        }
    }

    public int Social
    {
        get => social;
        set => social = Math.Clamp(value, 0, 5);
    }

    public int Technical
    {
        get => technical;
        set => technical = Math.Clamp(value, 0, 5);
    }

    public int Creative
    {
        get => creative;
        set => creative = Math.Clamp(value, 0, 5);
    }
    
    public int Trust
    {
        get => trust;
        set => trust = Math.Clamp(value, 0, 100);
    }

    /// <summary>
    /// A dictionary containing assignments delegated to this character and the time remaining to complete them.
    /// </summary>
    public Dictionary<Assignment, TimeSpan> DelegatedAssignments => delegatedAssignments;
}





