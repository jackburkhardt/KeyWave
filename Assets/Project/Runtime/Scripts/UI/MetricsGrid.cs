using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class MetricsGrid : MonoBehaviour
{
    public enum DisplayCondition
    {
        All,
        HighLocationAffinity
    }
    
    public PointsFishBowl template;

    public void OnGameSceneStart()
    {
        if (GetComponentsInChildren<PointsFishBowl>().Length != Points.GetAllPointsTypes().Count)
        {
            DestroyMetrics();
            SetMetrics();
        }
    }
    
    private void SetMetrics()
    {
        foreach (Transform metric in transform)
        {
            if (metric == template.transform) continue;
            Destroy(metric.gameObject);
        }
        
        template.gameObject.SetActive(false);
        var metrics = Points.GetAllPointsTypes();
        foreach (var metric in metrics)
        {
            var newMetric = Instantiate(template, transform);
            newMetric.SetPointType(metric);
            newMetric.gameObject.SetActive(true);
        }
    }
    
    private void DestroyMetrics()
    {
        var metrics = transform;
        foreach (Transform metric in metrics)
        {
            if (metric == template.transform) continue;
            Destroy(metric.gameObject);
        }
    }

    private void OnEnable()
    {
      SetMetrics();
    }

    private void OnDisable()
    {
        DestroyMetrics();
    }

    public List<RectTransform> GetValidMetrics(DisplayCondition displayCondition = DisplayCondition.All, Location location = null)
    {
        var validMetrics = new List<RectTransform>();
        switch (displayCondition)
        {
            case DisplayCondition.All:
                foreach (Transform metric in transform)
                {
                    if (metric.GetComponent<PointsFishBowl>() == template) continue;
                    validMetrics.Add(metric.transform as RectTransform);
                }
                break;
            case DisplayCondition.HighLocationAffinity:
              
                foreach (Transform metric in transform)
                {
                    if (metric.GetComponent<PointsFishBowl>() == template) continue;
                    if (location != null && location.LookupInt($"{metric.GetComponent<PointsFishBowl>().GetPointType().Name} Affinity") > 0)
                    {
                        validMetrics.Add(metric.transform as RectTransform);
                    }
                }
                break;
        }

        return validMetrics;
    }
    
    public void EnableValidMetrics(DisplayCondition displayCondition = DisplayCondition.All, Location location = null)
    {
     
        foreach (Transform metric in transform)
        {
            metric.gameObject.SetActive(false);
        }
        
        var validMetrics = GetValidMetrics(displayCondition, location);
        foreach (var metric in validMetrics)
        {
            metric.gameObject.SetActive(true);
        }
    }
    
}
