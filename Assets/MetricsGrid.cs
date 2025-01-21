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
    public RectTransform engagementContainer;
    public RectTransform commitmentContainer;

    public List<RectTransform> GetValidMetrics(DisplayCondition displayCondition = DisplayCondition.All, Location location = null)
    {
        var validMetrics = new List<RectTransform>();
        switch (displayCondition)
        {
            case DisplayCondition.All:
                validMetrics.Add(credibiltyContainer);
                validMetrics.Add(wellnessContainer);
                validMetrics.Add(engagementContainer);
                validMetrics.Add(commitmentContainer);
                break;
            case DisplayCondition.HighLocationAffinity:
                if (int.Parse(location.AssignedField("Wellness Affinity").value) > 0)
                {
                    validMetrics.Add(wellnessContainer);
                }
                if (int.Parse(location.AssignedField("Engagement Affinity").value) > 0)
                {
                    validMetrics.Add(engagementContainer);
                }
                if (int.Parse(location.AssignedField("Commitment Affinity").value) > 0)
                {
                    validMetrics.Add(commitmentContainer);
                }
                if (int.Parse(location.AssignedField("Credibility Affinity").value) > 0)
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
        engagementContainer.gameObject.SetActive(false);
        commitmentContainer.gameObject.SetActive(false);
        
        
        var validMetrics = GetValidMetrics(displayCondition, location);
        foreach (var metric in validMetrics)
        {
            metric.gameObject.SetActive(true);
        }
    }
    
}
