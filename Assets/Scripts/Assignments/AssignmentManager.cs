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
    public class AssignmentManager : ScriptableObject
    {
        private string _assignmentPath;
        public static List<Assignment> _chapterAssignments = new List<Assignment>();
        
        private void Awake()
        {
            _assignmentPath = Application.streamingAssetsPath + "/GameData/Assignments/";
        }
        
        [YarnCommand("activate_assignment")]
        public static void ActivateAssignment(string name)
        {
            Assignment assignment = _chapterAssignments.FirstOrDefault(inactive => inactive.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError("ActivateAssignment: An assignment by the name of \"{name}\" could not be found.");
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
            Assignment assignment = _chapterAssignments.FirstOrDefault(inactive => inactive.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError("CompleteAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }

            assignment.State = AssignmentState.Completed;
            GameEvent.CompleteAssignment(assignment);
        }
        
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
            Assignment assignment = _chapterAssignments.FirstOrDefault(inactive => inactive.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError("FailAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }

            assignment.State = AssignmentState.Failed;
            GameEvent.FailAssignment(assignment);
        }
        
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
        
        private IEnumerator DoAssignmentCountdown(Assignment assignment, TMP_Text timeleftText)
        {
            timeleftText.enabled = true;
            var time = assignment.TimeToComplete;
            while (time > 0 && !assignment.Completed)
            {
                yield return new WaitForSeconds(1);
                time--;
                timeleftText.text = $"{time / 60}:{time % 60}";
            }

            if (!assignment.Completed)
            {
                FailAssignment(assignment.Name);
            }
        }

        private void Load() => _chapterAssignments = 
            DataManager.DeserializeData<List<Assignment>>($"{_assignmentPath}Chapter{GameManager.Chapter}.json");

        private void Save() =>
            DataManager.SerializeData(_chapterAssignments, $"{_assignmentPath}Chapter{GameManager.Chapter}.json");

        private void OnDestroy()
        {
        }
    }
}
