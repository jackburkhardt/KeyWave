using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using AssignmentType = Assignments.Assignment.AssignmentType;

namespace Assignments
{
    /// <summary>
    /// Add this component to the Content object for displaying the list of active assignments.
    /// </summary>
    public class AssignmentDisplay : MonoBehaviour
    {

        [SerializeField] private Object _assignmentListingPrefab;
        private Dictionary<AssignmentType, Sprite> _assignmentIcons = new Dictionary<AssignmentType, Sprite>();
        private const float resultDisplayTime = 3;
        
        private void Awake()
        {
            GameEvent.OnAssignmentActive += AddAssignmentToDisplay;
            GameEvent.OnAssignmentComplete += (a) => StartCoroutine(DisplaySuccess(a));
            GameEvent.OnAssignmentFail += (a) => StartCoroutine(DisplayFail(a));

            var loadedSprites = Resources.LoadAll<Sprite>("AssignmentIcons");
            foreach (var sprite in loadedSprites)
            {
                // this creates a strict reliance on file naming conventions
                _assignmentIcons.Add(Enum.Parse<AssignmentType>(sprite.name), sprite);
            }
        }

        private void OnAssignmentComplete(Assignment assignment)
        {
            
        }
        
        private void AddAssignmentToDisplay(Assignment assignment)
        {
            var newDisplay = Instantiate(_assignmentListingPrefab, transform) as GameObject;
        
            _assignmentIcons.TryGetValue(assignment.Type, out var icon);
            newDisplay.GetComponent<Image>().sprite = icon ? icon : _assignmentIcons[0];
        
            var fields = newDisplay.GetComponents<TMP_Text>();
            fields[0].text = assignment.Name;
            fields[1].text = assignment.Descriptor;

            if (assignment.IsTimed) StartCoroutine(DisplayCountdownText(assignment, fields[3]));

            StartCoroutine(UIManager.FadeIn(newDisplay.GetComponent<Renderer>()));
        }
    
        private void RemoveAssignmentFromDisplay(Assignment assignment)
        {
            var display = FindListing(assignment);
            StartCoroutine(UIManager.FadeOut(display.GetComponent<Renderer>(), 0.5f, true));
        }

        private IEnumerator DisplayCountdownText(Assignment assignment, TMP_Text text)
        {
            int timeLeft = assignment.TimeToComplete;

            while (timeLeft > 0)
            {
                if (assignment.Over) yield break;
                yield return new WaitForSeconds(1);
                var mins = timeLeft / 60;
                var secs = (timeLeft % 60).ToString("00");
                text.text = $"{mins}:{secs}";
                timeLeft--;
            }
        }

        private IEnumerator DisplaySuccess(Assignment assignment)
        {
            var text = FindListing(assignment).GetComponents<TMP_Text>()[3];
            text.color = Color.green;
            text.text = "COMPLETE!";
            
            yield return new WaitForSecondsRealtime(resultDisplayTime);
            RemoveAssignmentFromDisplay(assignment);
        }
        
        private IEnumerator DisplayFail(Assignment assignment)
        {
            var text = FindListing(assignment).GetComponents<TMP_Text>()[3];
            text.color = Color.red;
            text.text = "FAILED!";

            yield return new WaitForSecondsRealtime(resultDisplayTime);
            RemoveAssignmentFromDisplay(assignment);
        }

        private GameObject FindListing(Assignment assignment)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Assignment>().Name == assignment.Name)
                {
                    return child.gameObject;
                }
            }

            Debug.LogError($"FindListing: Unable to find UI listing for assignment \"{assignment.Name}\"!");
            return default;
        }

        private void OnDestroy()
        {
            GameEvent.OnAssignmentActive -= AddAssignmentToDisplay;
        }
    }
}