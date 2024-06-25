using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class CustomUIQuestLogWindow : StandardUIQuestLogWindow
{
    [SerializeField] private StandardUITextTemplateList _questTimeTemplate;
    [SerializeField] private StandardUITextTemplateList _previousQuestEntriesTemplate;
    
    // Start is called before the first frame update
    public override bool IsQuestVisible(string questTitle)
    {
        return !checkVisibleField || Lua.IsTrue("Quest[\"" + DialogueLua.StringToTableIndex(questTitle) + "\"].Visible == true");
    }

    protected override void InitializeTemplates()
    {
        base.InitializeTemplates();
        
        Tools.SetGameObjectActive(_questTimeTemplate.gameObject, false);
    }
    
    protected override void RepaintSelectedQuest(QuestInfo quest)
    {
        detailsPanelContentManager.Clear();
        if (quest != null)
        {
            // Title:
            var titleInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questHeadingTextTemplate);
            titleInstance.Assign(quest.Heading.text);
            Debug.Log(quest.Heading.text);
            detailsPanelContentManager.Add(titleInstance, questDetailsContentContainer);

            
           
            
    
            
            
           
            // Description:
            var descriptionInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questDescriptionTextTemplate);
            descriptionInstance.Assign(quest.Description.text);
            detailsPanelContentManager.Add(descriptionInstance, questDetailsContentContainer);
            
            
            //Time
            var timeInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplateList>(_questTimeTemplate);

            var timeTexts = new List<string>();
            var timeStartText =
                $"Time Started: {DialogueLua.GetQuestField(quest.Title, "Time Start").AsString}";
            timeTexts.Add(timeStartText);
            var timeCompleteText =  $"Time Finished:  {DialogueLua.GetQuestField(quest.Title, "Time Complete").AsString}";
            if (QuestLog.IsQuestSuccessful(quest.Title)) timeTexts.Add(timeCompleteText);
            
            timeInstance.Assign(timeTexts);
            detailsPanelContentManager.Add(timeInstance, questDetailsContentContainer);
            

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
