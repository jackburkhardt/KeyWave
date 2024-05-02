using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;
using UnityEngine.Events;

public class CustomUIMenuPanel : StandardUIMenuPanel
{
    public static List<string> CustomFields = new List<string>
    {
        "offsetCurve", 
        "timeEstimate",
        "responseMenuAnimator",
        "mousePointerHand"
    };
    
    public static Action<CustomResponseButton> OnCustomResponseButtonClick;

    public static void ButtonClick(CustomResponseButton button)
    {
        SelectedResponseButton = button;
        Instance.mousePointerHand.Freeze();
        OnCustomResponseButtonClick?.Invoke(button);
    } 

    public static CustomResponseButton SelectedResponseButton;

    [SerializeField] public AnimationCurve offsetCurve;
    
    [SerializeField] private UITextField timeEstimate;
    [SerializeField] Animator responseMenuAnimator;
    [SerializeField] private PointerArrow mousePointerHand;
    
    public float globalOffset;


    public static CustomUIMenuPanel Instance;
    private float maxVisibleDegreeSum = 85;

    public UnityEvent onHide;
    public UnityEvent onShow;
    

    public override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
    }

    protected override void OnEnable()
    { 
        base.OnEnable();
       onContentChanged.AddListener(OnContentChanged);
    }

    protected override void OnDisable()
    { 
        base.OnDisable();
       onContentChanged.RemoveListener(OnContentChanged);
    }
    
    private void OnContentChanged()
    {
        mousePointerHand.Unfreeze();
        CustomResponseButton.RefreshButtonColors();
        foreach (var button in FindObjectsOfType<CustomResponseButton>())
        {
          button.SetResponseText();
        }
    }
    

    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        if (!Field.FieldExists(dialogueEntry.fields, "Time Estimate")) return "";
        
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        if (estimate.Item1 > estimate.Item2) return "";
        if (estimate.Item1 == estimate.Item2) return $"{estimate.Item1 / 60} minutes";
        return $"{estimate.Item1 / 60}-{estimate.Item2 / 60} minutes";
    }

    public void OnQuestStateChange(string questTitle)
    {
       // Debug.Log("questChange");
    }

    public void OnConversationEnd()
    {
        responseMenuAnimator.SetTrigger("Hide");
        Debug.Log("ending");
        //onHide.Invoke();
        //gameObject.SetActive(false);
    }

    protected override void Update()
    { 
        base.Update();
        
        if (CircularUIButton.CircularUIDegreeSum < maxVisibleDegreeSum)
        {
            CircularUIButton.globalOffset = 0;
            return;
        }
        
        var normalizedPointerAngle = mousePointerHand.AngleCenteredSouth / maxVisibleDegreeSum * 2f; 
        var offsetRange = CircularUIButton.CircularUIDegreeSum - maxVisibleDegreeSum;
        var offset = offsetRange * - (offsetCurve.Evaluate(Mathf.Abs(normalizedPointerAngle)) *  MathF.Sign(normalizedPointerAngle) * 0.5f); 
        CircularUIButton.globalOffset = offset;


        }
    }
    

    

