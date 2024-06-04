using System.Collections;
using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [ReadOnly]
    public static int CurrentVisualizedTimeRaw;
    private string CurrentVisualizedTime => RemoveColon(Clock.To24HourClock(CurrentVisualizedTimeRaw));

    [SerializeField] private TMP_Text timeText;
    private int _clock;
    
    protected void Start()
    {
        CurrentVisualizedTimeRaw = GameStateManager.instance.gameState.Clock;
        timeText.text = CurrentVisualizedTime;
    }

    private bool timeIsUpdating;

    private void Update()
    {
        timeText.text = CurrentVisualizedTime;
        if (timeIsUpdating) return;

        if (CurrentVisualizedTimeRaw < Clock.CurrentTimeRaw)
        {
            BroadcastMessage("OnTimeChange", (CurrentVisualizedTimeRaw,Clock.CurrentTimeRaw));
            StartCoroutine(UpdateVisualizedTime(Clock.CurrentTimeRaw));
        }

  
    }

    private string RemoveColon(string time)
    {
        return time.Replace(":", " ");
    }
    
 

    IEnumerator UpdateVisualizedTime(int newTime)
    {
        timeIsUpdating = true;
        var timeDifference = newTime - CurrentVisualizedTimeRaw;
        while (CurrentVisualizedTimeRaw < newTime)
        { 
            if (CurrentVisualizedTimeRaw == 0) break; // handles overflow
           timeText.text = RemoveColon(Clock.To24HourClock(CurrentVisualizedTimeRaw)); 
           CurrentVisualizedTimeRaw += (int)(2 * Mathf.Pow((Mathf.Pow(timeDifference, 0.5f) / 10f), 2f));
           yield return new WaitForSeconds(0.01f);
        }

        CurrentVisualizedTimeRaw = newTime % Clock.HoursToSeconds(24);
        timeText.text = CurrentVisualizedTime;
        timeIsUpdating = false;
        
        BroadcastMessage("OnTimeChanged");
        
      }

  
  
}
