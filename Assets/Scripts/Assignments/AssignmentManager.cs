using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using AssignmentState = Assignments.Assignment.AssignmentState;

namespace Assignments
{
    public class AssignmentManager : MonoBehaviour
    {
        private string _assignmentPath;
        public static List<Assignment> ChapterAssignments = new List<Assignment>();
        
        private void Awake()
        {
            _assignmentPath = Application.streamingAssetsPath + "/GameData/Assignments/";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            GameEvent.OnChapterEnd += OnChapterEnd;
            GameEvent.OnTimeChange += OnTimeChange;
        }

        [YarnCommand("activate_assignment")]
        public static void ActivateAssignment(string name)
        {
            Assignment assignment = ChapterAssignments.Find(inactive => inactive.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError($"ActivateAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }
            
            assignment.Activate();
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
            Assignment assignment = ChapterAssignments.Find(active => active.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError($"CompleteAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }
            
            assignment.Complete();
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
            Assignment assignment = ChapterAssignments.Find(active => active.Name == name);
            if (assignment.Equals(default))
            {
                Debug.LogError($"FailAssignment: An assignment by the name of \"{name}\" could not be found.");
                return;
            }
            
            assignment.Fail();
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
        
        [YarnCommand("delegate_assignment")]
        public static void DelegateAssignment(string assignmentName, string characterName)
        {
            Assignment assignment = ChapterAssignments.Find(active => active.Name == assignmentName);
            if (assignment.Equals(default))
            {
                Debug.LogError($"DelegateAssignment: An assignment by the name of \"{assignmentName}\" could not be found.");
                return;
            }
            
            Character character = CharacterManager.Find(characterName);
            if (character.Equals(default)){
                Debug.LogError($"DelegateAssignment: A character by the name of \"{characterName}\" could not be found.");
                return;
            }

            if (!assignment.CanDelegate)
            {
                Debug.LogError($"DelegateAssignment: The assignment \"{assignmentName}\" cannot be delegated.");
                return;
            }

            character.DelegateAssignment(assignment);
        }

        [YarnCommand("undelegate_assignment")]
        public static void UndelegateAssignment(string assignmentName, string characterName)
        {
            Assignment assignment = ChapterAssignments.Find(active => active.Name == assignmentName);
            if (assignment.Equals(default))
            {
                Debug.LogError($"UndelegateAssignment: An assignment by the name of \"{assignmentName}\" could not be found.");
                return;
            }
            
            Character character = CharacterManager.Find(characterName);
            if (character.Equals(default)){
                Debug.LogError($"UndelegateAssignment: A character by the name of \"{characterName}\" could not be found.");
                return;
            }

            character.UndelegateAssignment(assignment);
        }
        
        private void OnChapterEnd(int chapter)
        {
            foreach (var assignment in ChapterAssignments.Where(assignment => !assignment.Over))
            {
                FailAssignment(assignment.Name);
            }
        }

        private void OnTimeChange(TimeSpan time)
        {
            foreach (var assignment in ChapterAssignments.Where(
                         assignment => assignment.State is AssignmentState.Inactive  && assignment.ReleaseTime != default))
            {
                if (time >= assignment.ReleaseTime)
                {
                    assignment.Activate();
                }
            }
        }

        private void Load() => ChapterAssignments = 
            DataManager.DeserializeData<List<Assignment>>($"{_assignmentPath}Chapter{RealtimeManager.Chapter}.json");

        private void Save() =>
            DataManager.SerializeData(ChapterAssignments, $"{_assignmentPath}Chapter{RealtimeManager.Chapter}.json");

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
            GameEvent.OnChapterEnd -= OnChapterEnd;
            GameEvent.OnTimeChange -= OnTimeChange;
        }
    }
}
