using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class CustomUIQuestLogWindow : StandardUIQuestLogWindow
{
    [SerializeField] private StandardUITextTemplateList _questTimeTemplate;
    [SerializeField] private StandardUITextTemplate _previousQuestEntriesContainer;
    
    // Start is called before the first frame update
    public override bool IsQuestVisible(string questTitle)
    {
        return !checkVisibleField || Lua.IsTrue("Quest[\"" + DialogueLua.StringToTableIndex(questTitle) + "\"].Visible == true");
    }

    protected override void InitializeTemplates()
    {
        base.InitializeTemplates();
        
        Tools.SetGameObjectActive(_questTimeTemplate.gameObject, false);
        Tools.SetGameObjectActive(_previousQuestEntriesContainer.gameObject, false);
    }
    
    protected override void RepaintSelectedQuest(QuestInfo quest)
    {
        detailsPanelContentManager.Clear();
        if (quest != null)
        {
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
            
            
            // Title:
            var titleInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questHeadingTextTemplate);
            titleInstance.Assign(quest.Heading.text);
            Debug.Log(quest.Heading.text);
            detailsPanelContentManager.Add(titleInstance, questDetailsContentContainer);
            
           
            // Description:
            var descriptionInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questDescriptionTextTemplate);
            descriptionInstance.Assign(quest.Description.text);
            detailsPanelContentManager.Add(descriptionInstance, questDetailsContentContainer);
            

            // Active Entries:
            
            var activeEntries = quest.Entries.Where(e => quest.EntryStates[quest.Entries.ToList().IndexOf(e)] == QuestState.Active).ToList();

            for (int i = 0; i < activeEntries.Count; i++)
            {
                var entryInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(questEntryActiveTextTemplate);
                entryInstance.Assign(activeEntries[i].text);
                detailsPanelContentManager.Add(entryInstance, questDetailsContentContainer);
            }
            
            // Previous Entries
            
            var previousEntries = quest.Entries.Where(e => 
                quest.EntryStates[quest.Entries.ToList().IndexOf(e)] == QuestState.Success 
                || quest.EntryStates[quest.Entries.ToList().IndexOf(e)] == QuestState.Failure).ToList();
            
            if (previousEntries.Count > 0)
            {
                var previousEntriesTitleInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(_previousQuestEntriesContainer);
               
                detailsPanelContentManager.Add(previousEntriesTitleInstance, questDetailsContentContainer);
                
                for (int i = 0; i < quest.Entries.Length; i++)
                {
                    if (quest.EntryStates[i] != QuestState.Success && quest.EntryStates[i] != QuestState.Failure) continue;
                    var entryTemplate = GetEntryTemplate(quest.EntryStates[i]);
                    if (entryTemplate != null)
                    {
                        var entryInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(entryTemplate);
                        entryInstance.Assign(quest.Entries[i].text);
                        detailsPanelContentManager.Add(entryInstance, previousEntriesTitleInstance.GetComponent<RectTransform>());
                    }
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
