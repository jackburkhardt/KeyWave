using System.Collections.Generic;

namespace Assignments
{
    public class Assignment
    {
        public string Name { get; set; }
        public string Descriptor { get; set; }
        public AssignmentType Type { get; set; }
        public AssignmentState State { get; set; }
        public int TimeToComplete { get; private set; } // in seconds
        public List<string> DependentAssignments { get; set; }

        public Assignment(string name, string descriptor, AssignmentType type, AssignmentState state, int timeToComplete = 0, List<string> dependentAssignments = null)
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