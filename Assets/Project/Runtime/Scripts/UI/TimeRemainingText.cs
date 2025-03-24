using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class TimeRemainingText : MonoBehaviour
{
    public UITextField textField;
    private void Update()
    {
        var currentTime = ClockUI.CurrentVisualizedTimeRaw;
        
        
        
        var timeRemaining = Clock.DayEndTime - currentTime;
        if (timeRemaining < 0)
        {
            timeRemaining = 0;
        }
        
        var minutesRemaining = timeRemaining / 60;
        
        var hours = minutesRemaining / 60;
        var minutes = minutesRemaining % 60;
        
        textField.text = $"{hours:00} hours, {minutes:00} minutes left";
        
    }
}
