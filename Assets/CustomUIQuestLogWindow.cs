using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class CustomUIQuestLogWindow : StandardUIQuestLogWindow
{
    [SerializeField] private StandardUITextTemplate _questTimeStartTemplate;
    [SerializeField] private StandardUITextTemplate _questTimeCompleteTemplate;
    
    // Start is called before the first frame update
    public override bool IsQuestVisible(string questTitle)
    {
        return !checkVisibleField || Lua.IsTrue("Quest[\"" + DialogueLua.StringToTableIndex(questTitle) + "\"].Visible == true");
    }

    protected override void InitializeTemplates()
    {
        base.InitializeTemplates();
        
        Tools.SetGameObjectActive(_questTimeStartTemplate.gameObject, false);
        Tools.SetGameObjectActive(_questTimeCompleteTemplate.gameObject, false);
    }
    
    protected override void RepaintSelectedQuest(QuestInfo quest)
    {
        detailsPanelContentManager.Clear();
        if (quest != null)
        {
            // Title:
            var titleInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questHeadingTextTemplate);
            titleInstance.Assign(quest.Heading.text);
            detailsPanelContentManager.Add(titleInstance, questDetailsContentContainer);
            
            // Time

            if (!QuestLog.IsQuestUnassigned(quest.Title))
            {
                var timeStartInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(_questTimeStartTemplate);
                Debug.Log(DialogueManager.masterDatabase.GetQuest(quest.Title)!.FieldExists("Time Start"));
                var timeStartText =
                    $"Time Started: {DialogueManager.masterDatabase.GetQuest(quest.Title)?.AssignedField("Time Start").value}";
                timeStartInstance.Assign(timeStartText);
                detailsPanelContentManager.Add(timeStartInstance, questDetailsContentContainer);
            }
            
            if (QuestLog.IsQuestSuccessful(quest.Title))
            {
                var timeCompleteInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(_questTimeCompleteTemplate);
                var timeCompleteText =
                    $"Time Completed: {DialogueManager.masterDatabase.GetQuest(quest.Title)?.AssignedField("Time Complete").value}";
                timeCompleteInstance.Assign(timeCompleteText);
                detailsPanelContentManager.Add(timeCompleteInstance, questDetailsContentContainer);
            }
           
            // Description:
            var descriptionInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questDescriptionTextTemplate);
            descriptionInstance.Assign(quest.Description.text);
            detailsPanelContentManager.Add(descriptionInstance, questDetailsContentContainer);

            // Entries:
            for (int i = 0; i < quest.Entries.Length; i++)
            {
                var entryTemplate = GetEntryTemplate(quest.EntryStates[i]);
                if (entryTemplate != null)
                {
                    var entryInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(entryTemplate);
                    entryInstance.Assign(quest.Entries[i].text);
                    detailsPanelContentManager.Add(entryInstance, questDetailsContentContainer);
                }
            }

            // Abandon button:
            if (currentQuestStateMask == QuestState.Active && QuestLog.IsQuestAbandonable(quest.Title))
            {
                var abandonButtonInstance = detailsPanelContentManager.Instantiate<StandardUIButtonTemplate>(abandonButtonTemplate);
                detailsPanelContentManager.Add(abandonButtonInstance, questDetailsContentContainer);
                abandonButtonInstance.button.onClick.AddListener(ClickAbandonQuestButton);
            }
        }
    }
    
    
}
