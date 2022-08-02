using System.Collections.Generic;

namespace Assignments
{
    public struct Assignment
    {
        public string Name;
        public string Descriptor;
        public AssignmentType Type;
        public AssignmentState State;
        public int TimeToComplete; // in seconds
        public List<Assignment> DependentAssignments;

        public Assignment(string name, string descriptor, AssignmentType type, AssignmentState state, int timeToComplete, List<Assignment> dependentAssignments)
        {
            Name = name;
            Descriptor = descriptor;
            Type = type;
            State = state;
            TimeToComplete = timeToComplete;
            DependentAssignments = dependentAssignments;
        }

        public bool IsTimed => this.Type is AssignmentType.Timed or AssignmentType.PlayerTimed
            or AssignmentType.Emergency or AssignmentType.PlayerEmergency;

        public bool Over => this.State is AssignmentState.Completed or AssignmentState.Inactive or AssignmentState.Failed;

        public bool Completed => this.State == AssignmentState.Completed;
        
        public enum AssignmentType
        {
            General,
            Locked,
            PlayerOnly,
            Timed,
            Emergency,
            PlayerTimed,
            PlayerEmergency
        }

        public enum AssignmentState
        {
            Inactive,
            Active,
            Completed,
            Failed
        }
    }
}