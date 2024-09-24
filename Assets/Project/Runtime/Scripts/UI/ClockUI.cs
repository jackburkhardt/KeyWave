using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    public class ClockUI : MonoBehaviour
    {
        [ReadOnly]
        public static int CurrentVisualizedTimeRaw;

        public Color clockEnabledColor;
        public Color clockDisabledColor;

        public Image clock;

        [SerializeField] private TMP_Text timeText;
        private int _clock;

        [SerializeField] private UnityEvent OnHourChanged;
       
        
        private bool timeIsUpdating;
        private string CurrentVisualizedTime => RemoveColon(Clock.To24HourClock(CurrentVisualizedTimeRaw));

        private void OnEnable()
        {
            Clock.OnTimeScaleChange += OnTimeScaleChange;
        }

        private void OnDisable()
        {
            Clock.OnTimeScaleChange -= OnTimeScaleChange;
        }

        protected void Start()
        {
            CurrentVisualizedTimeRaw = GameStateManager.instance.gameState.Clock;
            timeText.text = CurrentVisualizedTime;
        }

        private void OnTimeScaleChange()
        {
            if (Clock.TimeScales.Modifier == 0)
            {
                clock.color = clockDisabledColor;
            }
            else
            {
                clock.color = clockEnabledColor;
            }
        }

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

        private List<string> _hoursRecorded = new();

        IEnumerator UpdateVisualizedTime(int newTime)
        {
            var startTime = CurrentVisualizedTime;
            timeIsUpdating = true;
            var timeDifference = newTime - CurrentVisualizedTimeRaw;
            while (CurrentVisualizedTimeRaw < newTime)
            { 
                if (CurrentVisualizedTimeRaw == 0) break; // handles overflow
                timeText.text = RemoveColon(Clock.To24HourClock(CurrentVisualizedTimeRaw)); 
                CurrentVisualizedTimeRaw += (int)(2 * Mathf.Pow((Mathf.Pow(timeDifference, 0.5f) / 10f), 2f));
                if (CurrentVisualizedTime.EndsWith("00") && CurrentVisualizedTime != startTime && !_hoursRecorded.Contains(CurrentVisualizedTime))
                {
                    _hoursRecorded.Add(CurrentVisualizedTime);
                    Debug.Log("Hour changed to " + CurrentVisualizedTime);
                    OnHourChanged?.Invoke();
                    
                }
                yield return new WaitForSeconds(0.01f);
            }

            CurrentVisualizedTimeRaw = newTime % Clock.HoursToSeconds(24);
            timeText.text = CurrentVisualizedTime;
            timeIsUpdating = false;
        
            BroadcastMessage("OnTimeChanged");

            DialogueManager.instance.PlaySequence("SequencerMessage(ClockUpdated)");

        }

        public void OnConversationStart()
        {
            if (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.GetConversation().Title != "Intro")
            GetComponent<Animator>().SetTrigger("Show");
        }
    }
}