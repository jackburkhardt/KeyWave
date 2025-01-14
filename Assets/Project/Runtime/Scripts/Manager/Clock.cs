using System;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.Manager
{
    public class Clock
    {
        public enum Hour
        {
            _0 = 0,
            _1 = 1,
            _2 = 2,
            _3 = 3,
            _4 = 4,
            _5 = 5,
            _6 = 6,
            _7 = 7,
            _8 = 8,
            _9 = 9,
            _10 = 10,
            _11 = 11,
            _12 = 12,
            _13 = 13,
            _14 = 14,
            _15 = 15,
            _16 = 16,
            _17 = 17,
            _18 = 18,
            _19 = 19,
            _20 = 20,
            _21 = 21,
            _22 = 22,
            _23 = 23
        }

        public enum Minute
        {
            _00 = 0,
            _01 = 1,
            _02 = 2,
            _03 = 3,
            _04 = 4,
            _05 = 5,
            _06 = 6,
            _07 = 7,
            _08 = 8,
            _09 = 9,
            _10 = 10,
            _11 = 11,
            _12 = 12,
            _13 = 13,
            _14 = 14,
            _15 = 15,
            _16 = 16,
            _17 = 17,
            _18 = 18,
            _19 = 19,
            _20 = 20,
            _21 = 21,
            _22 = 22,
            _23 = 23,
            _24 = 24,
            _25 = 25,
            _26 = 26,
            _27 = 27,
            _28 = 28,
            _29 = 29,
            _30 = 30,
            _31 = 31,
            _32 = 32,
            _33 = 33,
            _34 = 34,
            _35 = 35,
            _36 = 36,
            _37 = 37,
            _38 = 38,
            _39 = 39,
            _40 = 40,
            _41 = 41,
            _42 = 42,
            _43 = 43,
            _44 = 44,
            _45 = 45,
            _46 = 46,
            _47 = 47,
            _48 = 48,
            _49 = 49,
            _50 = 50,
            _51 = 51,
            _52 = 52,
            _53 = 53,
            _54 = 54,
            _55 = 55,
            _56 = 56,
            _57 = 57,
            _58 = 58,
            _59 = 59
        }

        public const int StartTime = 21600;

        public const int MaxDayTime = 72000;

        public static int CurrentVisualizedTimeRaw = ClockUI.CurrentVisualizedTimeRaw;

        public static int CurrentTimeRaw
        {
            get { return GameManager.gameState != null ? GameManager.gameState.Clock : DialogueLua.GetVariable("clock").asInt; }
            set { GameManager.gameState.Clock = value; }
        }

        public static float DayProgress {
            get
            {
                return (CurrentTimeRaw - StartTime)/(float)MaxDayTime;
            }
        }

        public static string CurrentTime => To24HourClock(CurrentTimeRaw);

        public static int DailyLimit => ToSeconds("20:00");

        public static void Freeze(bool freeze)
        {
            TimeScales.Modifier = freeze ? 0 : 1;
        }

        public static int HoursToSeconds(int hours) => hours * 3600;

        /// <summary>
        /// Converts a string in the format "HH:MM" to seconds. e.g. "07:30" to 27000
        /// </summary>
        /// <param name="hoursMinutes">A string with exaclty five characters: two digits, followed by a colon, followed by two digits.</param>
        /// <returns></returns>
        public static int ToSeconds(string hoursMinutes)
        {
            var split = hoursMinutes.Split(':');
        
            if (split.Length != 2 || split[0].Length > 2 || split[1].Length != 2) Debug.LogError($"Invalid time format. {hoursMinutes} does not match HH:MM");
            if (split[0].Length == 1) split[0] = "0" + split[0];
        
            var hours = int.Parse(split[0]);
            var minutes = int.Parse(split[1]);
        
            return (hours * 3600 + minutes * 60);
        }
        
        public static void AddSeconds(int seconds)
        {
            CurrentTimeRaw += seconds;
        }

        /// <summary>
        /// Converts a time in seconds to a string in the format "HH:MM". e.g. 27000 to "07:30"
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>

        public static string To24HourClock(int clock)
        {
            if (clock > MaxDayTime) clock -= MaxDayTime;
            var hours = clock / 3600;
            var minutes = (clock % 3600) / 60;
        
            var minutesString = minutes < 10 ? $"0{minutes}" : minutes.ToString();
            var hoursString = hours < 10 ? $"0{hours}" : hours.ToString();
        
            return $"{hoursString}:{minutesString}";
        }

        public static string EstimatedTimeOfArrival(Location location)
        {
            return To24HourClock(location.TravelTime + CurrentTimeRaw);
        }

        public static int EstimatedTimeOfArrivalRaw(Location location)
        {
            return location.TravelTime + CurrentTimeRaw;
        }


        public static int GetHoursAsInt(string time)
        {
            Debug.Log(time);
            return int.Parse(time.Split(":")[0]);
        }
        
        public static int GetHoursAsInt(int time)
        {
            return GetHoursAsInt(To24HourClock(time));
        }
        
        public static Action OnTimeScaleChange;


        public struct TimeScales
        {
            private static int _modifier = 1;
            internal static int Modifier
            {
                get
                {
                    return _modifier;
                }
                set
                {
                    _modifier = value;
                    OnTimeScaleChange.Invoke();
                }
            }
            internal static int GlobalTimeScale => 1 * Modifier;
    
            internal static float SecondsPercharacter = 3f; //default 1.5
            internal static int SecondsBetweenLines = 60;
            internal static int SecondsPerInteract = 45;
        }
    }
}


public class SequencerCommandAddSeconds : SequencerCommand
{
    public void Awake()
    {
        var time = GetParameterAsInt(0);
        Clock.AddSeconds(time);
        Stop();
    }
}

public class SequencerCommandAddMinutes: SequencerCommand
{
    public void Awake()
    {
        var time = GetParameterAsInt(0);
        Clock.AddSeconds(time * 60);
        Stop();
    }
}

public class SequencerCommandSetTime : SequencerCommand
{
    public void Awake()
    {
        var time = GetParameter(0);
       // var endOfLine = GetParameterAsBool(1, true);
        Clock.CurrentTimeRaw = Clock.ToSeconds(time);
       // if (endOfLine) sequencer.PlaySequence("EndOfLine()@Message(ClockUpdated)");
        Stop();
    }
}

public class SequencerCommandBlackOut : SequencerCommand
{
    private void Start()
    {
        var time = GetParameterAsInt(0);
        var unit = GetParameter(1, "minutes");
        var timeString = GetParameter(0, "");
        var playEndOfLine = GetParameterAsBool(2, true);

        int timeToAdd;
        
        switch (unit)
        {
            case "seconds":
                timeToAdd = time;
                break;
            case "minutes":
                timeToAdd = time * 60;
                break;
            case "hours":
                timeToAdd = time * 3600;
                break;
            case "set":
                timeToAdd = Mathf.Max(Clock.ToSeconds(timeString) - Clock.CurrentTimeRaw, 0);
                break;
            default:
                timeToAdd = time * 60;
                break;
        }

        var sequence = $"Fade(stay, 1);" +
                       $"SetContinueMode(false);" +
                       $"AddSeconds({timeToAdd})@1;" +
                       $"Delay(1)@Message(ClockUpdated)->Message(FadeOut);" +
                       $"Fade(unstay, 1)@Message(FadeOut)";
        
        if (playEndOfLine) sequence += "->Message(PlayEndOfLine);EndOfLine()@Message(PlayEndOfLine);";
        
        sequencer.PlaySequence(sequence);
        Stop();
    }
}
