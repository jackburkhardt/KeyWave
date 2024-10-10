using System;
using System.Collections;
using System.Collections.Generic;
using Project;
using Project.Runtime.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;

public class TimeSelectionInputPanel : MonoBehaviour
{
    
    [GetComponent]
    public TMP_InputField inputField ;

    [SerializeField] private TextMeshProUGUI title;
    
    private string _titleText = "Select a Time";
    public string TitleText
    {
        get
        {
            var title = _titleText;
            _titleText = "Select a Time";
            return title;
        }
        set
        {
            _titleText = value;
        }
    }

    public int increment = 20;
    
    public Image container;
 
    private TextMeshProUGUI Placeholder => inputField.placeholder.GetComponent<TextMeshProUGUI>();
    
    public Button hourUp;
    public Button hourDown;
    public Button minuteUp;
    public Button minuteDown;
    public Button submit;
    
    [ReadOnly] public string luaVariableName;
    
    [SerializeField] [ReadOnly] private int _inputTimeInt;

    public bool isOpen => container.gameObject.activeSelf;

    [ReadOnly] public bool openedFromDialogueSystem = false;
    
    private string _playSequenceOnSubmit = "";
    
    public Animator animator;
    
    public string PlaySequenceOnSubmit
    {
        get
        {
            if (string.IsNullOrEmpty(_playSequenceOnSubmit))
            {
                return $"EndOfLine()";
            }

            else
            {
                var sequence = _playSequenceOnSubmit;
                _playSequenceOnSubmit = "";
                return sequence;
            }
        }
        set => _playSequenceOnSubmit = value;
    }

    public int InputTimeInt
    {
        get => _inputTimeInt;
        set
        {
            
            var minutes = int.Parse(Clock.To24HourClock(value).Split(':')[1]);
           
            
            if (minutes % increment != 0)
            {
                value -= (minutes % increment) * 60;
            }
            
            _inputTimeInt = Mathf.Clamp(value, Clock.ToSeconds(EarliestSelectableTime), Clock.ToSeconds("23:59"));
            inputField.text = Clock.To24HourClock(_inputTimeInt);
            
            
            if (InputTimeInt == Clock.ToSeconds(EarliestSelectableTime)) inputField.text = string.Empty;
            
            SetButtonVisibilities();
            }

    }

    private string _earliestSelectableTime = string.Empty;
    
    public string EarliestSelectableTime {
        get =>
            Application.isPlaying
                ? string.IsNullOrEmpty(_earliestSelectableTime)
                    ? Clock.CurrentTime
                    : Clock.CurrentTimeRaw > Clock.ToSeconds(_earliestSelectableTime)
                        ? Clock.CurrentTime
                        : _earliestSelectableTime
                : "06:00";
        set => _earliestSelectableTime = value;
    }
    
    private string _latestSelectableTime = string.Empty;
    
    public string LatestSelectableTime
    {
        get =>
            string.IsNullOrEmpty(_latestSelectableTime)
                ? "23:59"
                : _latestSelectableTime;
               
        set => _latestSelectableTime = value;
    }

    private void Awake()
    {
        container.gameObject.SetActive(false);
        
    }

    private void OnValidate()
    {
        if (inputField != null)
        {
            Placeholder.text = EarliestSelectableTime;
            if (InputTimeInt == Clock.ToSeconds(EarliestSelectableTime)) MinuteUp();
        }
        
        if (hourUp == null || hourDown == null || minuteUp == null || minuteDown == null) return;
        
    //   Debug.Log("OnValidate");
         SetButtonVisibilities();
      //hourUp.gameObject.SetActive();
    }
    
    private bool SetButtonVisibility(Button button, bool visible)
    {
        if (button.GetComponent<CanvasGroup >() == null) 
            button.gameObject.AddComponent<CanvasGroup>();
        
        button.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
        button.interactable = visible;
        return visible;
    }
    
    private void SetButtonVisibilities()
    {
        SetButtonVisibility(hourUp, InputTimeInt + 3600 < Clock.ToSeconds(LatestSelectableTime));
        
        if (hourUp.onClick.GetPersistentEventCount() == 0)
        {
            hourUp.onClick.AddListener(HourUp);
        }
        
        SetButtonVisibility(hourDown, InputTimeInt - 3600 >= Clock.ToSeconds(EarliestSelectableTime));
        
        if (hourDown.onClick.GetPersistentEventCount() == 0)
        {
            hourDown.onClick.AddListener(HourDown);
        }
        
        SetButtonVisibility(minuteUp, InputTimeInt != Clock.ToSeconds(LatestSelectableTime));
        
        if (minuteUp.onClick.GetPersistentEventCount() == 0)
        {
            minuteUp.onClick.AddListener(MinuteUp);
        }
        
        SetButtonVisibility(minuteDown, InputTimeInt != Clock.ToSeconds(EarliestSelectableTime) && _earliestSelectableTime == string.Empty || _earliestSelectableTime != string.Empty && InputTimeInt - 60 * increment > Clock.ToSeconds(EarliestSelectableTime));
        
        if (minuteDown.onClick.GetPersistentEventCount() == 0)
        {
            minuteDown.onClick.AddListener(MinuteDown);
        }
        
        SetButtonVisibility(submit, InputTimeInt != Clock.ToSeconds(EarliestSelectableTime));
    }

    [Button]
    private void Reset()
    { DialogueLua.SetVariable(luaVariableName, "");
       InputTimeInt = Clock.ToSeconds(EarliestSelectableTime);
     
       OnValidate();
    }

    [Button]
    
    public void HourUp()
    {
        InputTimeInt += 3600;
    }
    [Button]
    public void HourDown()
    {
        InputTimeInt -= 3600;
       
    }
    [Button]
    public void MinuteUp()
    {
        InputTimeInt += 60 * increment;
       
    }
    [Button]
    public void MinuteDown()
    {
        InputTimeInt -= 60 * increment;
      
    }
    
    public void Open()
    {
        container.gameObject.SetActive(true);
        title.text = TitleText;
        if (luaVariableName != string.Empty) DialogueLua.SetVariable(luaVariableName, "");
        Reset();
        animator.SetTrigger("Show");
    }
    
    public void Close()
    {
        animator.SetTrigger("Hide");
        //DialogueManager.instance.PlaySequence("SequencerMessage(OnTimeSelectionPanelClose)");
        StartCoroutine(DisableContainerAfterCloseAnimation());
        openedFromDialogueSystem = false;
        LatestSelectableTime = string.Empty;
        EarliestSelectableTime = string.Empty;
    }

    private IEnumerator DisableContainerAfterCloseAnimation()
    {
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Show"))
        {
            yield return null;
        }
        
        container.gameObject.SetActive(false);
        
    }

    public void OnSubmit()
    {
        DialogueLua.SetVariable(luaVariableName, Clock.To24HourClock(InputTimeInt));
        
        IEnumerator PlaySequenceAfterOneFrame(string sequence)
        {
            yield return new WaitForEndOfFrame();
            while (DialogueLua.GetVariable(luaVariableName).asString != Clock.To24HourClock(InputTimeInt))
            {
                yield return null;
            }
            DialogueManager.instance.PlaySequence(sequence);
          
        }
        
        if (openedFromDialogueSystem || _playSequenceOnSubmit != string.Empty)
        {
            var sequence = PlaySequenceOnSubmit;
            StartCoroutine(PlaySequenceAfterOneFrame(sequence));
        }
        Close();
    }
    
    
    
    public void OnCancel()
    {
        DialogueLua.SetVariable(luaVariableName, string.Empty);
        
        IEnumerator PlaySequenceAfterOneFrame(string sequence)
        {
            yield return new WaitForEndOfFrame();
            while (DialogueLua.GetVariable(luaVariableName).asString != string.Empty)
            {
                yield return null;
            }
            DialogueManager.instance.PlaySequence(sequence);
          
        }
        
        if (openedFromDialogueSystem)
        {
            if (_playSequenceOnSubmit != string.Empty) StartCoroutine(PlaySequenceAfterOneFrame(PlaySequenceOnSubmit));
        }
        Close();
    }
    
    
    
}


public class SequencerCommandTimeSelectionPanel : SequencerCommand
{
    private void Awake()
    {
        sequencer.PlaySequence("SetContinueMode(false);");
        TimeSelectionInputPanel panel = FindObjectOfType<TimeSelectionInputPanel>();
        
        panel.TitleText =  GetParameter(0, "Enter a Time");;
        panel.increment = GetParameterAsInt(1, 20);
        panel.luaVariableName =  GetParameter(2, "TimeSelectionInputValue");
        panel.EarliestSelectableTime = GetParameter(3, "");
        panel.LatestSelectableTime = GetParameter(4, "");
        panel.openedFromDialogueSystem = true;
        panel.PlaySequenceOnSubmit = $"EndOfLine({sequencer.entrytag})";
        
        panel.Open();
    }
}