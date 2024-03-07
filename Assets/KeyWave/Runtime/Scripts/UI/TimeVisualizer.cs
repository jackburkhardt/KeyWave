using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PlayerEvent = PlayerEvents.PlayerEvent;

public class TimeVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    private int _clock;
    
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
   
    
    protected void Start()
    {
        StartCoroutine(ShowClockDelay());
    }
    
    IEnumerator ShowClockDelay()
    {
        if (_canvasGroup != null && _verticalLayoutGroup != null) {
            _canvasGroup.alpha = 0;
            yield return new WaitForEndOfFrame();
            _verticalLayoutGroup.enabled = false;
            yield return new WaitForEndOfFrame();
            _verticalLayoutGroup.enabled = true;
            _canvasGroup.alpha = 1;
        }
        _clock = GameManager.instance.gameStateManager.gameState.clock;
        timeText.text = ConvertClockToHourMinutes(_clock);
    }

    private bool timeIsUpdating;

    private void Update()
    {
        if (_clock != Clock.CurrentTimeRaw)
        {
            StartCoroutine(UpdateTimeTextHandler(Clock.CurrentTimeRaw));
            _clock = Clock.CurrentTimeRaw;
        }
    }

    IEnumerator UpdateTimeTextHandler(int time)
    {
        while (timeIsUpdating) yield return null;
        yield return UpdateTimeText(time);
    }

    IEnumerator UpdateTimeText(int newTime)
    {
        timeIsUpdating = true;
        var currentTime = _clock;
        var timeDifference = newTime - currentTime;
        while (currentTime < newTime)
        {
           timeText.text = ConvertClockToHourMinutes(currentTime);
           currentTime += (int)(2 * Mathf.Pow((Mathf.Pow(timeDifference, 0.5f) / 10f), 2f));
           yield return new WaitForSeconds(0.01f);
        }
        
        timeText.text = ConvertClockToHourMinutes(Clock.CurrentTimeRaw);
        timeIsUpdating = false;

    }

  
    private string ConvertClockToHourMinutes(int clock)
    {
        var hour = clock / 3600;
        var minutes = (clock % 3600) / 60;
        var hourString = hour < 10 ? $"0{hour}" : $"{hour}";
        var minutesString = minutes < 10 ? $"0{minutes}" : $"{minutes}";
        return $"{hourString} {minutesString}";
    }
}
