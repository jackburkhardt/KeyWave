using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ItemUIButton : MonoBehaviour
{
    /// <summary>
    /// Button that handles the display of an information of an Item, such as emails.
    /// </summary>
    
    
    [HelpBox("If Button's OnClick() event is empty, this Standard UI Response Button component will automatically assign its OnClick method at runtime. If Button's OnClick() event has other elements, you *must* manually assign the StandardUIResponseButton.OnClick method to it.", HelpBoxMessageType.Info)]
    public UnityEngine.UI.Button button;
   
    
    // <summary>
    /// Gets or sets the target that will receive click notifications.
    /// </summary>
    public virtual Transform target { get; set; }
    
    public virtual Item item { get; set; }

    public UITextField name;
    public UITextField description;
    
    public UITextField actorField;
    private bool actorFieldVisible => actorField != null && actorField.gameObject != null;
    [ShowIf("actorFieldVisible")] public string actorFieldName;
    
    [Tooltip("Object to enable when the item is active.")]
    public GameObject activeStateIndicator;

    
    
    public void SetItem(Item item)
    {
        if (name != null) name.text = item.Name;
        if (description != null) description.text = item.Description;

        if (item.FieldExists("State"))
        {
            var itemState = QuestLog.GetQuestState(item.Name);
            if (activeStateIndicator != null) activeStateIndicator.SetActive(itemState == QuestState.Active);
        }
        
        if (actorFieldVisible)
        {
            var actor = item.LookupActor(actorFieldName, DialogueManager.masterDatabase);
            if (actor != null) actorField.text = actor.Name;
        }
        
        this.item = item;
    }
    
    public void OnClick()
    {
        
        if (item.FieldExists("State"))
        {
            QuestLog.SetQuestState(item.Name, QuestState.Success);
            if (activeStateIndicator != null) activeStateIndicator.SetActive(false);
        }
        
        target.SendMessage("OnClick", item, SendMessageOptions.RequireReceiver);
    }

 

    public void OnEnable()
    {
        if (button.onClick.GetPersistentEventCount() == 0)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnValidate()
    {
        button ??= GetComponent<UnityEngine.UI.Button>();
    }
}
