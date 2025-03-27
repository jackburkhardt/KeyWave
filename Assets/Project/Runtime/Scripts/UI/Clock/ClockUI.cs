using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
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

        [SerializeField] private Color _hourChangedColor;
        [SerializeField] private Animator _animator;
       
        
        private bool timeIsUpdating;
        private string CurrentVisualizedTime => Clock.To24HourClock(CurrentVisualizedTimeRaw);

        private void OnEnable()
        {
            Clock.onTimeChange += OnTimeChange;
        }
        
        private void OnDisable()
        {
            Clock.onTimeChange -= OnTimeChange;
        }


        private void Awake()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected void Start()
        {
            CurrentVisualizedTimeRaw = Clock.CurrentTimeRaw;
            timeText.text = RemoveColon(CurrentVisualizedTime);
        }

        private void OnTimeChange(Clock.TimeChangeData timeChangeData)
        {

            if (timeChangeData.newTime == CurrentVisualizedTimeRaw)
            {
                DialogueManager.instance.PlaySequence("SequencerMessage(ClockUpdated)@1");
                return;
            }
            
            if (timeIsUpdating) return;
         //   BroadcastMessage("OnTimeChange", (CurrentVisualizedTimeRaw,Clock.CurrentTimeRaw));
            StartCoroutine(UpdateVisualizedTime(Clock.CurrentTimeRaw));
        }

        public void OnGameSceneStart()
        {
            Debug.Log("Game scene start: currentTimeRaw is " + Clock.CurrentTimeRaw);
            if (CurrentVisualizedTimeRaw != Clock.CurrentTimeRaw)
            {
                StartCoroutine(UpdateVisualizedTime(Clock.CurrentTimeRaw));
            }
        }

        private void Update()
        {
            timeText.text = RemoveColon(CurrentVisualizedTime);
        }

        private string RemoveColon(string time)
        {
            return time.Replace(":", " ");
        }

        private int _mostRecentHour = Clock.ToSeconds("06:00");

        IEnumerator UpdateVisualizedTime(int newTime)
        {
            var startTime = CurrentVisualizedTime;
            timeIsUpdating = true;
            var timeDifference = newTime - CurrentVisualizedTimeRaw;
            var hourChanged = false;
            
            while (CurrentVisualizedTimeRaw < newTime)
            { 
                if (CurrentVisualizedTimeRaw == 0) break; // handles overflow
                timeText.text = RemoveColon(Clock.To24HourClock(CurrentVisualizedTimeRaw)); 
                CurrentVisualizedTimeRaw += (int)(2 * Mathf.Pow((Mathf.Pow(timeDifference, 0.5f) / 10f), 2f));

                var currentTimeEndsInDoubleZero = CurrentVisualizedTimeRaw - 3600 > _mostRecentHour
                                                  && CurrentVisualizedTime != startTime
                                                  && Clock.GetHoursAsInt(CurrentVisualizedTime) >
                                                  Clock.GetHoursAsInt(_mostRecentHour)
                                                  && Clock.GetHoursAsInt(CurrentVisualizedTime) -
                                                  Clock.GetHoursAsInt(newTime) == 0;
                
                if (currentTimeEndsInDoubleZero && !_animator.GetCurrentAnimatorStateInfo(1).IsName("Scale"))
                {
                    _mostRecentHour = Clock.GetHoursAsInt(CurrentVisualizedTime) * 3600;
                    CurrentVisualizedTimeRaw = _mostRecentHour;
                    var defaultColor = timeText.color;
                    timeText.color = _hourChangedColor;
                    OnHourChanged?.Invoke();
                    
                    //yield return new WaitForSeconds(2);
                    if (!hourChanged)
                    {
                        hourChanged = true;
                        BroadcastMessage("OnTimeChanged");
                        DialogueManager.instance.PlaySequence("SequencerMessage(ClockUpdated)");
                    }
                    
                    //Debug.Log("Hour changed to " + CurrentVisualizedTime);
                    timeText.color = defaultColor;
                }
                yield return new WaitForSeconds(0.01f);
            }

            CurrentVisualizedTimeRaw = newTime % Clock.HoursToSeconds(24);
            timeText.text = RemoveColon(CurrentVisualizedTime);
            timeIsUpdating = false;
            
            

            if (_animator.GetCurrentAnimatorStateInfo(1).IsName("Scale"))
            {
                var defaultColor = timeText.color;
                timeText.color = _hourChangedColor;
                OnHourChanged?.Invoke();
                BroadcastMessage("OnTimeChanged");
                DialogueManager.instance.PlaySequence("SequencerMessage(ClockUpdated)");    
                yield return new WaitForSeconds(1);
                timeText.color = defaultColor;
                _animator.SetTrigger("Unscale");
            }
            
            else if (!hourChanged)
            {
                BroadcastMessage("OnTimeChanged");
                DialogueManager.instance.PlaySequence("SequencerMessage(ClockUpdated)");
            }

        }

        public void OnConversationStart()
        {
            if (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.GetConversation().Title != "Intro")
            _animator.SetTrigger("Show");
        }
    }
}