using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class EndOfDayTimelineElement : MonoBehaviour
{
    public enum ElementType
    {
        Text,
        Points
    }
    
    
    public ElementType elementType;
    
    public UITextField textField;
    
    [ShowIf("elementType", ElementType.Points)]
    public FakeTimelineData fakeTimelineData;
    
    public void OnTimelineInput(float value)
    {
        return;
        switch (elementType)
        {
            case ElementType.Text:
                textField.text = Clock.To24HourClock(Clock.TimeFromProgress(value));
                break;
            case ElementType.Points:
                textField.text = fakeTimelineData.OutputValue(value).ToString();
                break;
        }
        
    }
}
