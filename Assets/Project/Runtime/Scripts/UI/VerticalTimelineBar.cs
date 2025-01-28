using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class VerticalTimelineBar : MonoBehaviour
{
    public RectTransform content;

    public List<Bar> bars;
    
    public AnimationCurve curve;
    
    

    [Serializable]

    public class Bar
    {
        public RectTransform rect;
        public FakeTimelineData timelineData;
        [AllowNesting]
        [Label("Raw Value")]
        public float value;
        
    }
    private void OnValidate()
    {
        contentSizeCapacity = content.rect.height;
        capacityInClockSeconds = contentSizeCapacity * timelineConversion;
        percentageOfTotalClockCapacity = capacityInClockSeconds / (Clock.DayEndTime - Clock.DayStartTime);

        SetBars();
    }

    private void SetBars()
    {
        foreach (var bar in bars)
        {
            var height = valueSum <= maxValue ? bar.value : bar.value / valueSum * maxValue;
            bar.rect.sizeDelta = new Vector2(bar.rect.sizeDelta.x, height);

            var ascendingBars = new List<Bar>(bars);
            
            ascendingBars.Sort((a, b) => b.value.CompareTo(a.value));
            
            bar.rect.SetSiblingIndex(ascendingBars.IndexOf(bar));
        }
    }
    
    private float maxValue => content.rect.height;

    private float valueSum
    {
        get
        {
            var sum = 0f;
            foreach (var value in bars.Select(bar => bar.value))
            {
                sum += value;
            }
            return sum;
        }
    }

    public float timelineConversion;
    [ReadOnly] public float contentSizeCapacity;
    [ReadOnly] public float capacityInClockSeconds;
    [ReadOnly] public float percentageOfTotalClockCapacity;

    public void OnValidateInput(float input)
    {
        foreach (var bar in bars)
        {
            var timelineValue = bar.timelineData.OutputValue(input);
            bar.value = timelineValue / timelineConversion;
        }
        
        SetBars();
    }
    
    
}
