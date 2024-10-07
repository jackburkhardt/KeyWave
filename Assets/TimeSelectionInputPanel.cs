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

    public TextMeshProUGUI title;

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
            
            _inputTimeInt = Mathf.Clamp(value, Clock.ToSeconds(CurrentTime), Clock.ToSeconds("23:59"));
            inputField.text = Clock.To24HourClock(_inputTimeInt);
            
            
            if (InputTimeInt == Clock.ToSeconds(CurrentTime)) inputField.text = string.Empty;
            
            SetButtonVisibilities();
            }

    }
    
    private string CurrentTime => Application.isPlaying ? Clock.CurrentTime : "06:32";

    private void Awake()
    {
        container.gameObject.SetActive(false);
        
    }

    private void OnValidate()
    {
        if (inputField != null)
        {
            Placeholder.text = CurrentTime;
            if (InputTimeInt == Clock.ToSeconds(CurrentTime)) MinuteUp();
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
        SetButtonVisibility(hourUp, InputTimeInt + 3600 <= Clock.ToSeconds("23:59"));
        
        if (hourUp.onClick.GetPersistentEventCount() == 0)
        {
            hourUp.onClick.AddListener(HourUp);
        }
        
        SetButtonVisibility(hourDown, InputTimeInt - 3600 >= Clock.ToSeconds(CurrentTime));
        
        if (hourDown.onClick.GetPersistentEventCount() == 0)
        {
            hourDown.onClick.AddListener(HourDown);
        }
        
        SetButtonVisibility(minuteUp, InputTimeInt != Clock.ToSeconds("23:59"));
        
        if (minuteUp.onClick.GetPersistentEventCount() == 0)
        {
            minuteUp.onClick.AddListener(MinuteUp);
        }
        
        SetButtonVisibility(minuteDown, InputTimeInt != Clock.ToSeconds(CurrentTime));
        
        if (minuteDown.onClick.GetPersistentEventCount() == 0)
        {
            minuteDown.onClick.AddListener(MinuteDown);
        }
        
        SetButtonVisibility(submit, InputTimeInt != Clock.ToSeconds(CurrentTime));
    }

    [Button]
    private void Reset()
    { DialogueLua.SetVariable(luaVariableName, "");
       InputTimeInt = Clock.ToSeconds(CurrentTime);
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
    
    public void Show()
    {
        container.gameObject.SetActive(true);
        Reset();
    }
    
    public void Hide()
    {
        container.gameObject.SetActive(false);
    }
    
    public void OnSubmit()
    {
        DialogueLua.SetVariable(luaVariableName, Clock.To24HourClock(InputTimeInt));
        DialogueManager.instance.PlaySequence("Continue()");
        Hide();
    }
    
    public void OnCancel()
    {
        DialogueLua.SetVariable(luaVariableName, "");
        DialogueManager.instance.PlaySequence("Continue()");
        Hide();
    }
    
}


public class SequencerCommandTimeSelectionPanel : SequencerCommand
{
    private void Awake()
    {
        sequencer.PlaySequence("SetContinueMode(false);");
        var text = GetParameter(0, "Enter a Time");
        var increment = GetParameterAsInt(1, 20);
        var variable = GetParameter(2, "TimeSelectionInputValue");

        TimeSelectionInputPanel panel = FindObjectOfType<TimeSelectionInputPanel>();
        panel.title.text = text;
        panel.increment = increment;
        panel.luaVariableName = variable;
        panel.Show();

    }
}