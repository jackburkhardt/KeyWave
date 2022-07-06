using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Assignment : MonoBehaviour
{
    [SerializeField] private AssignmentType _assignmentType;
    public bool _locked;
    [SerializeField] private int _timeToComplete; // in seconds
    public List<Assignment> _dependentAssignments;
    public string descriptor;
    [SerializeField] private bool _completed;

    public void Activate()
    {
        if (_assignmentType is AssignmentType.TimeSensitive or AssignmentType.PlayerTimeSensitive)
        {
            StartCoroutine(TimedAssignmentCountdown());
        }
        
    }
    
    private IEnumerator TimedAssignmentCountdown()
    {
        yield return new WaitForSeconds(_timeToComplete);
        if (!_completed) Fail();
    }
    
    public void Complete()
    {
        _completed = true;
    }

    public void Fail()
    {
        
    }

    public AssignmentType AssignmentType => _assignmentType;
}