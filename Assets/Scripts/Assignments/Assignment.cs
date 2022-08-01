using System.Collections.Generic;

namespace Assignments
{
 
    public class Assignment
    {
        public string Name;
        public string Descriptor;
        public AssignmentType Type;
        public bool Locked;
        public int TimeToComplete; // in seconds
        public List<Assignment> DependentAssignments;
        public bool Completed;

        public Assignment(string name, string descriptor, AssignmentType type, bool locked, int timeToComplete, List<Assignment> dependentAssignments, bool completed)
        {
            Name = name;
            Descriptor = descriptor;
            Type = type;
            Locked = locked;
            TimeToComplete = timeToComplete;
            DependentAssignments = dependentAssignments;
            Completed = completed;
        }
        

        public bool IsTimed => Type is AssignmentType.TimeSensitive or AssignmentType.PlayerTimeSensitive
            or AssignmentType.Emergency or AssignmentType.PlayerEmergency;
    }
}