using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEndOfDayEvent : MonoBehaviour
{
    public UnityEvent onEndOfDay;

    public void OnEndOfDay()
    {
        onEndOfDay.Invoke();
    }
}
