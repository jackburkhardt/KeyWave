using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgendaWidget : MonoBehaviour
{
    
    public NotificationWidget objectivePrefab;
    
    public 
    // Start is called before the first frame update
    void Start()
    {
        objectivePrefab.gameObject.SetActive(false);
    }
    
    void AddObjectiveToAgenda(string objectiveTitle)
    {
        var newObjective = Instantiate(objectivePrefab, transform);
        newObjective.GetComponent<NotificationWidget>().SetObjective(objectiveTitle);
        newObjective.gameObject.SetActive(true);
    }
    
    void RefreshAgenda()
    {
        foreach (NotificationWidget widget in GetComponentsInChildren<NotificationWidget>())
        {
            if (widget.GetObjectiveState() != GameObjective.State.unassigned)
            {
               widget.gameObject.SetActive(true);
            }
        }
    }
    
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
