using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using AssignmentType = Assignments.Assignment.AssignmentType;
using AssignmentState = Assignments.Assignment.AssignmentState;

namespace Assignments
{
    public class AssignmentManager : MonoBehaviour
    {
        private string _assignmentPath;
        [NonSerialized] public static List<Assignment> _chapterAssignments = new List<Assignment>();
        
        private void Awake()
        {
            _assignmentPath = Application.streamingAssetsPath + "/GameData/Assignments/";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            GameEvent.OnAssignmentActive += assignment =>
            {
                if (assignment.IsTimed) StartCoroutine(DoAssignmentCountdown(assignment));
            };
            GameEvent.OnChapterEnd += OnChapterEnd;

            //AssignmentTest();
        }

        /*void AssignmentTest()
        {
            Assignment newass1 = new Assignment("Getting Started", "Log into your computer.", AssignmentType.General,
                AssignmentState.Active);
            Assignment newass2 = new Assignment("A New Idea", "Rob told you to check your email.",
                AssignmentType.General, AssignmentState.Inactive);
            _chapterAssignments.Add(newass1);
            _chapterAssignments.Add(newass2);
            ActivateAssignment(newass1.Name);
        }*/
        
        [YarnCommand("activate_assignment")]
        public static void ActivateAssignment(string name)
        {
            Assignment assignment = _chapterAssignments.FirstOrDefault(inactive => inactive.Name == name);
            if (assignment.Equals(default) || assignment.State is AssignmentState.Completed or AssignmentState.Failed)
            {
                Debug.LogError($"ActivateAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }

            assignment.State = AssignmentState.Active;
            GameEvent.StartAssignment(assignment);
        }
        
        public static void ActivateAssignment(IEnumerable<string> names)
        {
            foreach (var s in names)
            {
                ActivateAssignment(s);
            }
        }

        [YarnCommand("complete_assignment")]
        public static void CompleteAssignment(string name)
        {
            Assignment assignment = _chapterAssignments.FirstOrDefault(active => active.Name == name);
            if (assignment.Equals(default) || assignment.Over)
            {
                Debug.LogError($"CompleteAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }

            assignment.State = AssignmentState.Completed;
            GameEvent.CompleteAssignment(assignment);
        }
        
        /// <summary>
        /// Override for CompleteAssignment to allow for multiple assignments to be completed at once.
        /// </summary>
        public static void CompleteAssignment(IEnumerable<string> names)
        {
            foreach (var s in names)
            {
                CompleteAssignment(s);
            }
        }

        [YarnCommand("fail_assignment")]
        public static void FailAssignment(string name)
        {
            Assignment assignment = _chapterAssignments.FirstOrDefault(active => active.Name == name);
            if (assignment.Equals(default) || assignment.Over)
            {
                Debug.LogError($"FailAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }

            assignment.State = AssignmentState.Failed;
            GameEvent.FailAssignment(assignment);
        }
        
        /// <summary>
        /// Override for FailAssignment to allow for multiple assignments to be failed at once.
        /// </summary>
        public static void FailAssignment(IEnumerable<string> names)
        {
            foreach (var s in names)
            {
                FailAssignment(s);
            }
        }
        
        public static void DelegateAssignment(Assignment assignment, Character character)
        {
            if (assignment.Type is AssignmentType.PlayerOnly or AssignmentType.PlayerTimed
                or AssignmentType.PlayerEmergency) return;

            character.TryRecieveAssignment(assignment); // TODO: handle if this fails
        }
        
        /// <summary>
        /// For timed assignments, this function will wait until the due time and if the assignment is
        /// not completed, it will be failed.
        /// </summary>
        private IEnumerator DoAssignmentCountdown(Assignment assignment)
        {
            yield return new WaitUntil(() => RealtimeManager.Time > assignment.DueTime);

            if (!assignment.Completed)
            {
                FailAssignment(assignment.Name);
            }
        }

        private void OnChapterEnd(int chapter)
        {
            foreach (var assignment in _chapterAssignments.Where(assignment => !assignment.Over))
            {
                FailAssignment(assignment.Name);
            }
        }

        private void Load() => _chapterAssignments = 
            DataManager.DeserializeData<List<Assignment>>($"{_assignmentPath}Chapter{RealtimeManager.Chapter}.json");

        private void Save() =>
            DataManager.SerializeData(_chapterAssignments, $"{_assignmentPath}Chapter{RealtimeManager.Chapter}.json");

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
            GameEvent.OnChapterEnd -= OnChapterEnd;
        }
    }
}
