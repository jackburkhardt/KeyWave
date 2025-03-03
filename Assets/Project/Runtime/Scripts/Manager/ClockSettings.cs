using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Manager
{
    public class Clock
    {
        public static int CurrentTimeRaw
        {
            get { return GameManager.gameState.Clock; }
            set { GameManager.gameState.Clock = value; }
        }

        public static float DayProgress {
            get
            {
                float range = GameManager.settings.Clock.DayEndTime - GameManager.settings.Clock.DayStartTime;
                return (CurrentTimeRaw - GameManager.settings.Clock.DayStartTime)/range;
            }
        }

        public static string CurrentTime => To24HourClock(CurrentTimeRaw);
        
        public static int TimeFromProgress(float progress)
        {
            return Mathf.RoundToInt(progress * (GameManager.settings.Clock.DayEndTime - GameManager.settings.Clock.DayStartTime) + GameManager.settings.Clock.DayStartTime);
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

        public static string To24HourClock(int clock, bool includeSeconds = false)
        {
            while (clock > HoursToSeconds(24)) clock -= HoursToSeconds(24);
            var hours = clock / 3600;
            var minutes = (clock % 3600) / 60;
            var seconds = clock % 60;
        
            var minutesString = minutes < 10 ? $"0{minutes}" : minutes.ToString();
            var hoursString = hours < 10 ? $"0{hours}" : hours.ToString();
            var secondsString = seconds < 10 ? $"0{seconds}" : seconds.ToString();
        
            return includeSeconds ? $"{hoursString}:{minutesString}:{secondsString}" : $"{hoursString}:{minutesString}";
        }
        

        public static string EstimatedTimeOfArrival(int locationID)
        {
            return To24HourClock((int)GameManager.DistanceToLocation(locationID) + CurrentTimeRaw);
        }

        public static int GetHoursAsInt(string time)
        {
            return int.Parse(time.Split(":")[0]);
        }
        
        public static int GetHoursAsInt(int time)
        {
            return GetHoursAsInt(To24HourClock(time));
        }
        
        public static int DayStartTime => GameManager.settings.Clock.DayStartTime;
        public static int DayEndTime => GameManager.settings.Clock.DayEndTime;
        
        public static int SecondsPerCharacter => Mathf.RoundToInt(GameManager.settings.Clock.SecondsPerCharacter * GameManager.settings.Clock.globalModifier);
        public static int SecondsBetweenLines => Mathf.RoundToInt(GameManager.settings.Clock.SecondsBetweenLines * GameManager.settings.Clock.globalModifier);
        public static int SecondsPerInteract => Mathf.RoundToInt(GameManager.settings.Clock.SecondsPerInteract * GameManager.settings.Clock.globalModifier);
        
        
    }
}

[CreateAssetMenu(fileName = "Clock Settings", menuName = "Settings/Clock Settings")]

public class ClockSettings : ScriptableObject
{
    public float globalModifier = 1;
    
    private static string secondsPerCharacterKey = "game.clock.secondsPerCharacter";
    private static string secondsBetweenLinesKey = "game.clock.secondsBetweenLines";
    private static string secondsPerInteractKey = "game.clock.secondsPerInteract";
    
    
    public float SecondsPerCharacter;
    public int SecondsBetweenLines;
    public int SecondsPerInteract;
    
    public int DayStartTime = 21600;
    [ReadOnly] [Label("Time:")] public string DayStartTimeString = "06:00:00";
    public int DayEndTime = 72000;
    [ReadOnly]  [Label("Time:")] public string DayEndTimeString = "20:00:00";

    public bool showModifiableCurrentTime => !Application.isPlaying;
    [ShowIf("showModifiableCurrentTime")]
    [Tooltip("This property modifies the variable \"clock\" in the Dialogue Database")]
    [SerializeField] private int currentTime = 21600;
    [HideIf("showModifiableCurrentTime")]
    [InfoBox( "The current time cannot be directly modified in play mode.")]
    [ReadOnly] [SerializeField] [Label("Current Time")] private int readOnlyCurrentTime;
    [ReadOnly] [Label("Time:")] public string CurrentTimeString = "06:00:00";
    
    DialogueDatabase dialogueDatabase;

    private void OnValidate()
    {
        dialogueDatabase ??= GameManager.settings.dialogueDatabase;
        
        var secondsPerCharacterVariable = dialogueDatabase.GetVariable(secondsPerCharacterKey);
        if (secondsPerCharacterVariable == null)
        {
            secondsPerCharacterVariable = new Variable();
            secondsPerCharacterVariable.Name = secondsPerCharacterKey;
            dialogueDatabase.variables.Add(secondsPerCharacterVariable);
        }
        
        secondsPerCharacterVariable.InitialValue = SecondsPerCharacter.ToString();
        
        
        var secondsBetweenLinesVariable = dialogueDatabase.GetVariable(secondsBetweenLinesKey);
        if (secondsBetweenLinesVariable == null)
        {
            secondsBetweenLinesVariable = Template.FromDefault().CreateVariable(Template.FromDefault().GetNextVariableID(dialogueDatabase), secondsBetweenLinesKey, SecondsBetweenLines.ToString()); 
            GameManager.settings.dialogueDatabase.variables.Add(secondsBetweenLinesVariable);
        }
        
        secondsBetweenLinesVariable.InitialValue = SecondsBetweenLines.ToString();
        
        var secondsPerInteractVariable = dialogueDatabase.GetVariable(secondsPerInteractKey);
        if (secondsPerInteractVariable == null)
        {
            secondsPerInteractVariable = Template.FromDefault().CreateVariable(Template.FromDefault().GetNextVariableID(dialogueDatabase), secondsPerInteractKey, SecondsPerInteract.ToString()); 
            GameManager.settings.dialogueDatabase.variables.Add(secondsPerInteractVariable);
        }
        
        secondsPerInteractVariable.InitialValue = SecondsPerInteract.ToString();
        
        currentTime = Mathf.Clamp(currentTime, DayStartTime, DayEndTime);

        if (!Application.isPlaying)
        {
            if (GameManager.settings != null) GameManager.settings.dialogueDatabase.GetVariable("clock").InitialValue = currentTime.ToString();
        }
        
        else currentTime = DialogueLua.GetVariable("clock").asInt;
        
        readOnlyCurrentTime = currentTime;
        
        DayStartTimeString = Clock.To24HourClock(DayStartTime, true);
        DayEndTimeString = Clock.To24HourClock(DayEndTime, true);
        CurrentTimeString = Clock.To24HourClock(currentTime, true);
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

        sequencer.PlaySequence("SetAllContinueButtons(false)");
        
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
        
        if (playEndOfLine) sequence += "->Message(PlayEndOfLine);EndOfLine()@Message(PlayEndOfLine); SetAllContinueButtons(true)@Message(PlayEndOfLine);";
        
        else sequence += "->Message(SetContinueButtonsTrue);SetAllContinueButtons(true)@Message(SetContinueButtonsTrue);";
        
        sequencer.PlaySequence(sequence);
        Stop();
    }
    
}

public class SequencerCommandSetAllContinueButtons : SequencerCommand
{
    private void Start()
    {
        bool param = GetParameterAsBool(0, true);
        
        var continueButtons = FindObjectsByType<StandardUIContinueButtonFastForward>( FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var standardUIContinueButtonFastForward in continueButtons)
        {
            standardUIContinueButtonFastForward.enabled = param;
            standardUIContinueButtonFastForward.GetComponent<Button>().interactable = param;
        }
    }
}
