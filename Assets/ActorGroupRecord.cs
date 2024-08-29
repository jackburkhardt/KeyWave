using System;

namespace Project.Runtime
{

    /// <summary>
    /// Contains a quest title and its group name.
    /// </summary>
    public class ActorGroupRecord : IComparable
    {

        public string groupName;
        public string actorName;

        public ActorGroupRecord() { }

        public ActorGroupRecord(string groupName, string actorName)
        {
            this.groupName = groupName;
            this.actorName = actorName;
        }

        public int CompareTo(object obj)
        {
            var other = obj as ActorGroupRecord;
            if (other == null) return 1;
            if (string.Equals(this.groupName, other.groupName))
            {
                return string.Compare(this.actorName, other.actorName);
            }
            else
            {
                return string.Compare(this.groupName, other.groupName);
            }
        }

    }

}