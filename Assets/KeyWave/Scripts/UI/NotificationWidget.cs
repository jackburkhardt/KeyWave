using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class NotificationWidget : MonoBehaviour
{


    [SerializeField] private GameObjective _objective;
    [SerializeField] private TMPro.TMP_Text _taskText, _etaText;
 
    // Start is called before the first frame update

    public GameObjective.State GetObjectiveState()
    {
        return _objective.state;
    }

    public void SetObjective(GameObjective objective)
    {
        _objective = objective;
    }
    
    public void SetObjective(string objectiveTitle)
    {
        GameObjective objective = null;
        
        foreach (var obj in GameManager.instance.Objectives)
        {
            if (obj.objectiveTitle == objectiveTitle)
            {
                objective = obj;
            }
        }
        _objective = objective;
    }

    // Update is called once per frame
    void Update()
    {
        _taskText.text = $"{_objective.objectiveTitle} at the {_objective.location} at {_objective.hour}:{_objective.minute}";
        _etaText.text = $"Earliest ETA: {GameManager.instance.GetEtaToLocation(_objective.location)}";
    }
}
