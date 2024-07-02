using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestCompleteAlert : MonoBehaviour
{
    //[SerializeField] private Color onQuestActiveColor, onQuestUpdateColor = Color.white;
    [SerializeField] private Image image;
    [SerializeField] private UITextField taskName;
    [SerializeField] private UITextField questSuccessDescription;
    [SerializeField] private UIPanel panel;
    private string currentQuestName;
    public UnityEvent OnShowEvent;
    public UnityEvent OnHideEvent;
    // Start is called before the first frame update

    private void OnEnable()
    {
        Points.OnPointsAnimEnd += OnPointsAnimEnd;
    }

    private void OnDisable()
    {
        Points.OnPointsAnimEnd -= OnPointsAnimEnd;
    }

    private void OnPointsAnimEnd()
    {
        panel.Close();
    }

    void Alert(string title, string description)
    {
        taskName.text = title;
        questSuccessDescription.text = description;
        panel.Open();
        
        //material animations
        image.material.SetFloat("_BlurIntensity", 20f);
        image.material.DOFloat(0f, "_BlurIntensity", 1f);
        taskName.uiText.material.SetFloat("_OutlineSoftness", 1f);
        taskName.uiText.material.DOFloat(0f, "_OutlineSoftness", 1f);
        taskName.uiText.material.SetFloat("_FaceDilate", 1f);
        taskName.uiText.material.DOFloat(0f, "_FaceDilate", 1f);
    }
    
    void OnQuestStateChange(string questName)
    {
        var quest = DialogueManager.instance.masterDatabase.items.Find(p => p.Name == questName);
        if (quest.Group != "Main Task") return;
        if (QuestLog.IsQuestSuccessful(questName)) return;
        Alert(QuestLog.GetQuestTitle(questName), QuestLog.GetQuestDescription(questName));
        currentQuestName = questName;
    }

    public void OnContinue()
    {
        var bob = Instantiate<Image>(image);
        var quest = DialogueManager.masterDatabase.GetQuest(currentQuestName);
        var points =  DialogueUtility.GetPointsFromField(quest!.fields);
        GameEvent.OnPointsIncrease(points, currentQuestName);
    }
    
    public void OnShow()
    {
        OnShowEvent?.Invoke();
    }
    
    public void OnHide()
    {
        OnHideEvent?.Invoke();
    }
}