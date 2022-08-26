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
        [SerializeField] private float resultDisplayTime = 3;
        
        private void Start()
        {
            GameEvent.OnAssignmentActive += AddAssignmentToDisplay;
            GameEvent.OnAssignmentComplete += OnAssignmentComplete;
            GameEvent.OnAssignmentFail += OnAssignmentFail;

            var loadedSprites = Resources.LoadAll<Sprite>("AssignmentIcons");
            foreach (var sprite in loadedSprites)
            {
                // this creates a strict reliance on file naming conventions
                _assignmentIcons.Add(Enum.Parse<AssignmentType>(sprite.name), sprite);
            }

            foreach (var assignment in AssignmentManager.ChapterAssignments)
            {
                if (assignment.IsActive) AddAssignmentToDisplay(assignment);
            }
        }
        
        private void AddAssignmentToDisplay(Assignment assignment)
        {
            var newDisplay = Instantiate(_assignmentListingPrefab, transform) as GameObject;
        
            _assignmentIcons.TryGetValue(assignment.Type, out var icon);
            newDisplay.GetComponentsInChildren<Image>()[1].sprite = icon ? icon : _assignmentIcons[0];
            newDisplay.name = assignment.Name;
            
            var fields = newDisplay.GetComponentsInChildren<TMP_Text>();
            fields[0].text = assignment.Name;
            fields[1].text = assignment.Descriptor;

            if (assignment.IsTimed) fields[2].text = $"Due by: {assignment.DueTime}";

            // todo: update prefab alphas to 0
            StartCoroutine(UIManager.FadeRenderers(newDisplay.GetComponentsInChildren<Renderer>(), 0f, 1f));
        }

        private void RemoveAssignmentFromDisplay(Assignment assignment)
        {
            var assignmentDisplay = transform.Find(assignment.Name);
            if (assignmentDisplay == null) return;
            StartCoroutine(UIManager.FadeRenderers(assignmentDisplay.GetComponentsInChildren<Renderer>(), 1f, 0f));
            Destroy(assignmentDisplay.gameObject);
        }

        private void OnAssignmentComplete(Assignment assignment) => StartCoroutine(DisplaySuccess(assignment));
        private IEnumerator DisplaySuccess(Assignment assignment)
        {
            var text = FindListing(assignment).GetComponentsInChildren<TMP_Text>()[2];
            text.enabled = true;
            text.color = Color.green;
            text.text = "COMPLETE!";
            
            yield return new WaitForSecondsRealtime(resultDisplayTime);
            RemoveAssignmentFromDisplay(assignment);
        }
        
        private void OnAssignmentFail(Assignment assignment) => StartCoroutine(DisplayFail(assignment));
        private IEnumerator DisplayFail(Assignment assignment)
        {
            var text = FindListing(assignment).GetComponentsInChildren<TMP_Text>()[2];
            text.enabled = true;
            text.color = Color.red;
            text.text = "FAILED!";

            yield return new WaitForSecondsRealtime(resultDisplayTime);
            RemoveAssignmentFromDisplay(assignment);
        }

        private GameObject FindListing(Assignment assignment)
        {
            foreach (Transform child in transform)
            {
                if (child.name == assignment.Name)
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
            GameEvent.OnAssignmentComplete -= OnAssignmentComplete;
            GameEvent.OnAssignmentFail -= OnAssignmentFail;
        }
    }
}