using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assignments
{
    public class AssignmentManager : MonoBehaviour
    {
        [SerializeField] public static List<Assignment> _activeAssignments = new List<Assignment>();
        [SerializeField] public static List<Assignment> _inactiveAssignments;
        [SerializeField] public static List<Assignment> _completedAssignments = new List<Assignment>();
        
        [SerializeField] private Transform _assignmentsDisplay;
        [SerializeField] private Object _assignmentListingPrefab;
        private Dictionary<AssignmentType, Sprite> _assignmentIcons = new Dictionary<AssignmentType, Sprite>( );

        private void Awake()
        {
            GameEvent.OnChapterStart += OnChapterStart;
            GameEvent.OnAssignmentActive += AddAssignmentToDisplay;
            GameEvent.OnAssignmentComplete += RemoveAssignmentFromDisplay;
        }

        private void OnChapterStart(int chapter)
        {

        }
        
        public static void ActivateAssignment(string name)
        {
            Assignment assignment = _inactiveAssignments.FirstOrDefault(inactive => inactive.Name == name);
            if (assignment == default) return;
            
            _inactiveAssignments.Remove(assignment);
            _activeAssignments.Add(assignment);
            GameEvent.StartAssignment(assignment);
        }
        
        private void AddAssignmentToDisplay(Assignment assignment)
        {
            var newDisplay = Instantiate(_assignmentListingPrefab, _assignmentsDisplay) as GameObject;
        
            _assignmentIcons.TryGetValue(assignment.Type, out var icon);
            newDisplay.GetComponent<Image>().sprite = icon ? icon : _assignmentIcons[0];
        
            var fields = newDisplay.GetComponents<TMP_Text>();
            fields[0].text = assignment.Name;
            fields[1].text = assignment.Descriptor;

            if (assignment.IsTimed)
            {
                StartCoroutine(DoAssignmentCountdown(assignment, fields[3]));
            }

            StartCoroutine(UIManager.FadeIn(newDisplay.GetComponent<Renderer>()));
        }
    
        private void RemoveAssignmentFromDisplay(Assignment assignment)
        {
            foreach (Transform child in _assignmentsDisplay)
            {
                if (child.GetComponent<Assignment>().Name == assignment.Name)
                {
                    StartCoroutine(UIManager.FadeOut(child.GetComponent<Renderer>(), 0.5f, true));
                }
            }
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
                assignment.Fail();
            }
        }

        public static void DelegateAssignment(Assignment assignment, Character character)
        {
            if (assignment.Type is AssignmentType.PlayerOnly or AssignmentType.PlayerTimeSensitive
                or AssignmentType.PlayerEmergency) return;

            character.TryRecieveAssignment(assignment); // TODO: handle if this fails
        }

        private void OnDestroy()
        {
            GameEvent.OnChapterStart -= OnChapterStart;
            GameEvent.OnAssignmentActive += AddAssignmentToDisplay;
            GameEvent.OnAssignmentComplete += RemoveAssignmentFromDisplay;
        }
    }
}
