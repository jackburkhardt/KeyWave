using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class FakeTimelineData : MonoBehaviour, IEndOfDayTimelineDataHandler
{
    public enum FakeDataType
    {
        Points,
        Conversation
    }
    
   
    public FakeDataType fakeDataType;
    [ShowIf("fakeDataType", FakeDataType.Points)]
    public Points.Type pointsType;
    
   
    
    [Serializable]
    public class FakeDataPoint
    {
        [HideInInspector] public FakeDataType fakeDataType;
        public int time;
        [ReadOnly] public string timeString;
        [AllowNesting]
        [ShowIf("fakeDataType", FakeDataType.Points)]
        public int value;
        
        
        public enum ConversationEventType
        {
            Start,
            End
        }
        
        [AllowNesting]
        [ShowIf("fakeDataType", FakeDataType.Conversation)]
        public ConversationEventType conversationEventType; 
    }
    
    public List<FakeDataPoint> fakeDataPoints;

    [Range(0, 1)] public float timelineInput;
    
    [ReadOnly] public float outPutValue;
    
    public void OnTimelineInput(float value)
    {
        timelineInput = value;
    }

    public int OutputValue(float input)
    {
        var value = 0;

        switch (fakeDataType)
        {
            case FakeDataType.Points:
                for (int i = 0; i < fakeDataPoints.Count; i++)  {
                    var point = fakeDataPoints[i];
                    if (point.time <= Clock.TimeFromProgress(input)) value += point.value;
                    else break;
                    } ;
                break;
            
            case FakeDataType.Conversation:
                
                for (int i = 1; i < fakeDataPoints.Count; i++)
                {

                    if (fakeDataPoints[i].time <= Clock.TimeFromProgress(input))
                    {
                        if (fakeDataPoints[i].conversationEventType == FakeDataPoint.ConversationEventType.End
                            && fakeDataPoints[i-1].conversationEventType == FakeDataPoint.ConversationEventType.Start)
                        {
                            if (Clock.TimeFromProgress(input) >= fakeDataPoints[i-1].time && Clock.TimeFromProgress(input) <= fakeDataPoints[i].time)
                            {
                                value += (fakeDataPoints[i].time - fakeDataPoints[i-1].time);
                            }
                        }
                        
                        
                        else if (i == fakeDataPoints.Count - 1 && fakeDataPoints[i].conversationEventType == FakeDataPoint.ConversationEventType.Start)
                        {
                            value += (Clock.TimeFromProgress(input) - fakeDataPoints[i].time);
                        }
                        
                    }
                }
                break;
        }
        
        return value;
    }

    public void OnValidate()
    {
        
        for (int i = 0; i < fakeDataPoints.Count; i++) 
        {
            fakeDataPoints[i].fakeDataType = fakeDataType;
            var point = fakeDataPoints[i];

            if (i == 0) point.time = Math.Max(point.time, Clock.DayStartTime);
            
            else if (i == fakeDataPoints.Count - 1) point.time = Math.Clamp(point.time, fakeDataPoints[i - 1].time, Clock.DayEndTime);
            
            else point.time = Math.Clamp(point.time, fakeDataPoints[i - 1].time, fakeDataPoints[i + 1].time);
            
            point.time = Mathf.Clamp(point.time, Clock.DayStartTime, Clock.DayEndTime);
            point.timeString = Clock.To24HourClock(point.time);
            
        }
    }
}

public interface IEndOfDayTimelineDataHandler
{
    //void OnTimelineInput(float value);
}