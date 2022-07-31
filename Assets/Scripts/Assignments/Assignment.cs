using System.Collections.Generic;

namespace Assignments
{
 
    public class Assignment
    {
        public string Name;
        public AssignmentType Type;
        public bool Locked;
        public int TimeToComplete; // in seconds
        public List<Assignment> DependentAssignments;
        public string Descriptor;
        public bool Completed;

        public void Complete()
        {
            GameEvent.CompleteAssignment(this);
            Completed = true;
        }

        public void Fail()
        {
            GameEvent.FailAssignment(this);
        }

        public bool IsTimed => Type is AssignmentType.TimeSensitive or AssignmentType.PlayerTimeSensitive
            or AssignmentType.Emergency or AssignmentType.PlayerEmergency;
    }
}