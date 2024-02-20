using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using StandardUIResponseButton = PixelCrushers.DialogueSystem.Wrappers.StandardUIResponseButton;

public class CustomResponsePanel : MonoBehaviour
{
    [SerializeField] private UITextField timeEstimate;
    private DialogueEntry currentlySelectedDialogueEntry;

    public void SetCurrentResponseButton(StandardUIResponseButton responseButton)
    {
        
        currentlySelectedDialogueEntry = responseButton.response.destinationEntry;
        
        if (Field.LookupBool(currentlySelectedDialogueEntry.fields, "Time Estimate"))
        {
            timeEstimate.gameObject.SetActive(true);
            ShowTimeEstimate(currentlySelectedDialogueEntry);
            //print all links
            foreach (var link in currentlySelectedDialogueEntry.outgoingLinks)
            {
                Debug.Log(link.destinationConversationID);
            }
        }

        else
        {
            timeEstimate.gameObject.SetActive(false);
        }
        
    }
    
    public void UnsetResponseButton()
    {
        var anyButtonActive = false;

        foreach (var responseButton in GetComponentsInChildren(typeof(StandardUIResponseButton)))
        {
            if (responseButton.GetComponent<StandardUIResponseButton>().isButtonActive)
            {
                anyButtonActive = true;
            }
        }
        
        if (!anyButtonActive)
        {
            timeEstimate.gameObject.SetActive(false); 
        }
        
    }
        
    
    public void ShowTimeEstimate(DialogueEntry dialogueEntry)
    {
        var timeRange = GameManager.instance.FindDurationRange(dialogueEntry);
        
       
        
        if (timeRange.Item2 == timeRange.Item1)
        {
            timeEstimate.text = (timeRange.Item1/60).ToString();
        }
        else
        {
            timeEstimate.text = timeRange.Item1/60 + " - " + timeRange.Item2/60;
        }
        
        timeEstimate.text += " minutes";
    }
   
    // Start is called before the first frame update
    void Start()
    {
     timeEstimate.gameObject.SetActive(false);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
