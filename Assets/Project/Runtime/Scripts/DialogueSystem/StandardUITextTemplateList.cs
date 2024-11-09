using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class StandardUITextTemplateList : StandardUIContentTemplate
{
    public List<UITextField> textFields = new List<UITextField>();
    
    public virtual void Awake()
    {
        if (textFields.Count == 0)
        {
            foreach (var uiText in GetComponentsInChildren<Text>())
            {
                textFields.Add(new UITextField(uiText));
            }
           
            if (textFields.Count == 0 && Debug.isDebugBuild) Debug.LogError("Dialogue System: UI Text is unassigned.", this);
        }
    }

    public void Assign(List<string> texts)
    {
        for (int i = 0; i < textFields.Count; i++)
        {
            if (i >= texts.Count)
            {
                textFields[i].text = string.Empty;
                continue;
            }

            textFields[i].text = texts[i];
        }
    }
}
