using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class TrafficLevelElement : MonoBehaviour
{
    public Color activeColor;
    public Color inactiveColor;

    public string startTime;
    public string endTime;

    private int _startTimeInt;
    private int _endTimeInt;
    
    // Start is called before the first frame update
    void Start()
    {
        _startTimeInt = Clock.ToSeconds(startTime);
        _endTimeInt = Clock.ToSeconds(endTime);
        
        if (Clock.CurrentTimeRaw >= _startTimeInt && Clock.CurrentTimeRaw < _endTimeInt)
        {
            GetComponent<Image>().color = activeColor;
        }
        else
        {
            GetComponent<Image>().color = inactiveColor;
        }
    }

}
