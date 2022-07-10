using System.Collections.Generic;
using System.Linq;
using Interaction;
using UnityEngine;

namespace Assignments
{
    public class AssignmentManager : MonoBehaviour
    {
        [SerializeField] private List<Assignment> _activeAssignments = new List<Assignment>();
        [SerializeField] private List<Assignment> _inactiveAssignments;
        [SerializeField] private List<Assignment> _completedAssignments = new List<Assignment>();

        private void Awake()
        {
            GameEvent.OnChapterStart += OnChapterStart;
        }

        private void OnChapterStart()
        {
            // findobjects is not great but it's fine here
            _inactiveAssignments = FindObjectsOfType<Assignment>().ToList();
        }

        public void ActivateAssignment(Assignment assignment)
        {
            _inactiveAssignments.Remove(assignment);
            _activeAssignments.Add(assignment);
            assignment.Activate();
        }

        public void DelegateAssignment(Assignment assignment, Character character)
        {
            if (assignment.AssignmentType is AssignmentType.PlayerOnly or AssignmentType.PlayerTimeSensitive
                or AssignmentType.PlayerEmergency) return;

            character.TryRecieveAssignment(assignment); // TODO: handle if this fails
        }

        private void OnDestroy()
        {
            GameEvent.OnChapterStart -= OnChapterStart;
        }
    }
}
