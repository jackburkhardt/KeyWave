using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class MetricsGrid : MonoBehaviour
{
    public enum DisplayCondition
    {
        All,
        HighLocationAffinity
    }
    
    public RectTransform credibiltyContainer;
    public RectTransform wellnessContainer;
    public RectTransform TeamworkContainer;
    public RectTransform ContextContainer;

    public List<RectTransform> GetValidMetrics(DisplayCondition displayCondition = DisplayCondition.All, Location location = null)
    {
        var validMetrics = new List<RectTransform>();
        switch (displayCondition)
        {
            case DisplayCondition.All:
                validMetrics.Add(credibiltyContainer);
                validMetrics.Add(wellnessContainer);
                validMetrics.Add(TeamworkContainer);
                validMetrics.Add(ContextContainer);
                break;
            case DisplayCondition.HighLocationAffinity:
                if (int.Parse(location.AssignedField("Wellness Affinity").value) > 0)
                {
                    validMetrics.Add(wellnessContainer);
                }
                if (int.Parse(location.AssignedField("Teamwork Affinity").value) > 0)
                {
                    validMetrics.Add(TeamworkContainer);
                }
                if (int.Parse(location.AssignedField("Context Affinity").value) > 0)
                {
                    validMetrics.Add(ContextContainer);
                }
                if (int.Parse(location.AssignedField("Skills Affinity").value) > 0)
                {
                    validMetrics.Add(credibiltyContainer);
                }
                
                break;
        }

        return validMetrics;
    }
    
    public void EnableValidMetrics(DisplayCondition displayCondition = DisplayCondition.All, Location location = null)
    {
        credibiltyContainer.gameObject.SetActive(false);
        wellnessContainer.gameObject.SetActive(false);
        TeamworkContainer.gameObject.SetActive(false);
        ContextContainer.gameObject.SetActive(false);
        
        
        var validMetrics = GetValidMetrics(displayCondition, location);
        foreach (var metric in validMetrics)
        {
            metric.gameObject.SetActive(true);
        }
    }
    
}
