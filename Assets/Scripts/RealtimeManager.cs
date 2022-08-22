using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class RealtimeManager : MonoBehaviour
{
    public static int Chapter;
    private TimeSpan _timeSpan;
    private static bool _controlsEnabled = true;

    // the number of seconds between "steps" of time when game clock will progress
    private const int TIMESTEP_SECONDS = 20;
    // the amount of minutes that the game clock will progress at each step
    private const int TIMESTEP_REALTIME = 15;
    
    // the current time of the game clock (set to start of work day)
    private static TimeSpan time = new(9, 0,0);
    // end time of game clock (i.e end of work day)
    private readonly TimeSpan EndTime = new(17, 0,0);

    private void Awake()
    {
        //GameEvent.OnInteractionStart += obj => _controlsEnabled = false;
        //GameEvent.OnInteractionEnd += obj => _controlsEnabled = true;
    }

    private void Start()
    {
        GameEvent.StartChapter(Chapter);
        StartCoroutine(DoRealtimeClock());
    }

    private void Update()
    {
        if (!_controlsEnabled) return;

    }

    public static void PauseRealtime(bool toggle)
    {
        _controlsEnabled = !toggle;
        UnityEngine.Time.timeScale = toggle ? 0 : 1;
    }

    private IEnumerator DoRealtimeClock()
    {
        while (time < EndTime)
        {
            yield return new WaitForSeconds(TIMESTEP_SECONDS);
            time = time.Add(TimeSpan.FromMinutes(TIMESTEP_REALTIME)); // todo: finalize
        }
        
        GameEvent.EndChapter(Chapter);
    }

    public static TimeSpan Time => time;
    public static bool ControlsEnabled => _controlsEnabled;

}