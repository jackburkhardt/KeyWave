using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PlayerEvent = PlayerEvents.PlayerEvent;

public class TimeVisualizer : PlayerEventHandler
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
        _canvasGroup.alpha = 0;
        yield return new WaitForEndOfFrame();
        _verticalLayoutGroup.enabled = false;
        yield return new WaitForEndOfFrame();
        _verticalLayoutGroup.enabled = true;
        _canvasGroup.alpha = 1;
        
        _clock = GameManager.instance.gameStateManager.gameState.clock;
        timeText.text = ConvertClockToHourMinutes(_clock);
    }

    
    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
        StartCoroutine(UpdateTimeText(_clock + playerEvent.Duration));
    }

    IEnumerator UpdateTimeText(int newTime)
    {
       var timeDifference = newTime - _clock;
       while (_clock < newTime)
        {
            timeText.text = ConvertClockToHourMinutes(_clock);
            _clock += (int)(2 * Mathf.Pow((Mathf.Pow(timeDifference, 0.5f) / 10f), 2f));
            yield return new WaitForSeconds(0.01f);
        }
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
