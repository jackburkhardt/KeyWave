using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
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
