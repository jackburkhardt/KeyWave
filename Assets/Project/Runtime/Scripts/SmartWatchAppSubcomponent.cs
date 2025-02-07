using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;

public class SmartWatchAppSubcomponent : MonoBehaviour
{
    [Dropdown("apps")]
    public string app;
    private List<string> apps => SmartWatch.instance.appNames;
    
    private bool showCondition => conditionTypes.Count > 0;
    [ShowIf("showCondition")]
    [Dropdown("conditions")]
    public string condition;

    [InfoBox("This app does not have any classes assigned in this script. Edit this script to add a class and make sure the class name starts with '{AppName}App'", EInfoBoxType.Warning)]
    [HideIf("showCondition")]
    [Label("Condition")]
    [ReadOnly] public string warning;

    private int currentIndex = 0;
    
    private List<string> conditions => conditionTypes.Select(p => Regex.Replace(p.Name.Split("App")[1], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToList();
    private List<Type> conditionTypes => TypeFinder.FindAllClassesOfType<AppSubcomponent>().Where(p =>p.Name.StartsWith($"{app}App")).ToList();

    
    public void Evaluate<T>(T something)
    {
        var index = conditions.IndexOf(condition);
        var instance = (AppSubcomponent<T>)Activator.CreateInstance(conditionTypes[index]);
        instance.subcomponent = this;
        var result = instance.Evaluate(something, out var followup);
        if (result) followup?.Invoke();
    }
    
    public void Evaluate()
    {
        var index = conditions.IndexOf(condition);
        var instance = (AppSubcomponent)Activator.CreateInstance(conditionTypes[index]);
        instance.subcomponent = this;
        var result = instance.Evaluate(out var followup);
        if (result) followup?.Invoke();
    }

    public void OnValidate()
    {
        if (!conditions.Contains(condition) && !string.IsNullOrEmpty(condition))
        {
            //find string that most closely matches the current condition
            var closestMatch = conditions.OrderBy(p => LevenshteinDistance.Compute(p, condition)).First();
            condition = closestMatch;
        }
        
        currentIndex = conditions.IndexOf(condition);
    }
    
    
}

public class AppSubcomponent
{
    public SmartWatchAppSubcomponent subcomponent;
    public virtual bool Evaluate(out Action followup)
    {
        throw new NotImplementedException();
    }
    
}

public class AppSubcomponent<T> : AppSubcomponent
{
    public virtual bool Evaluate(T someData, out Action followup)
    {
        throw new NotImplementedException();
    }
    
}
        
public class ActionsAppEnableIfQuestRepeatableAndUntouched : AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        if (!standardUIResponseButton.response.TryGetQuest( out var quest)  || !standardUIResponseButton.button.interactable) return false;

        var repeatable = quest.IsFieldAssigned("Repeatable") && DialogueLua.GetQuestField(quest.Name, "Repeatable").asBool;
        var repeatableAndUntouched = repeatable && DialogueLua.GetQuestField(quest.Name, "Repeat Count").asInt == 0 || DialogueLua.GetQuestField(quest.Name, "Points Repeat").asString == "1";
        
        evalPassAction = () => subcomponent.gameObject.SetActive(repeatableAndUntouched);
        return true;
        
    }
    
   
}
        
public class ActionsAppEnableIfQuestRepeatableAndHasPointReduction : AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        
        if (!standardUIResponseButton.response.TryGetQuest( out var quest)  || !standardUIResponseButton.button.interactable) return false;

        var repeatable = quest.IsFieldAssigned("Repeatable") && DialogueLua.GetQuestField(quest.Name, "Repeatable").asBool;
        var repeatableAndDirty = repeatable && DialogueLua.GetQuestField(quest.Name, "Repeat Count").asInt > 0 && DialogueLua.GetQuestField(quest.Name, "Points Repeat").asFloat < 1;

        evalPassAction = () => subcomponent.gameObject.SetActive(repeatableAndDirty);
        return true;
    }
}
        
public class ActionsAppEnableIfQuestRewardsPoints :AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        if (!standardUIResponseButton.response.TryGetQuest( out var quest) || !standardUIResponseButton.button.interactable) return false;
        
        var rewardsPoints = quest.IsFieldAssigned("Points") &&  quest.fields.Any(p => p.title == "Points" && Points.PointsField.FromLuaField(p).Points > 0);
        
       // var rewardsPointsAndUntouched =
           // rewardsPoints && DialogueLua.GetQuestField(quest.Name, "Repeat Count").asInt == 0;
        
        evalPassAction = () => subcomponent.gameObject.SetActive(rewardsPoints);
        return true;
    }
}
        
public class ActionsAppEnableIfQuestHasFixedTimeCostAndReplaceText : AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        if (standardUIResponseButton.response == null ||  standardUIResponseButton.response.destinationEntry == null || !standardUIResponseButton.button.interactable) return false;
        
        var timespan = DialogueUtility.TimeEstimate(standardUIResponseButton.response.destinationEntry);

        var fixedTimeCost = timespan.Item1 == timespan.Item2;
        
        evalPassAction = () =>
        {
            var timeCost = fixedTimeCost ? timespan.Item1 : 0;
            subcomponent.gameObject.SetActive(fixedTimeCost && timeCost > 0);
            Debug.Log("Time cost: " + timeCost);
            
            var texts = subcomponent.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                text.text = text.text.Replace("{0}", $"{timespan.Item1 / 60}");
            }
        };

        return true;
    }
}
        
public class ActionsAppEnableIfQuestHasVariableTimeCostAndReplaceText : AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        if (standardUIResponseButton.response == null ||  standardUIResponseButton.response.destinationEntry == null || !standardUIResponseButton.button.interactable) return false;

        var timespan = DialogueUtility.TimeEstimate(standardUIResponseButton.response.destinationEntry);

        var variableTimeCost = timespan.Item1 != timespan.Item2;
        
        evalPassAction = () =>
        {
            subcomponent.gameObject.SetActive(variableTimeCost && timespan.Item2 > timespan.Item1);
            
            var texts = subcomponent.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                text.text = text.text.Replace("{0}", $"{timespan.Item1 / 60}-{timespan.Item2 / 60}");
            }
        };

        return true;
    }
}


public class ActionsAppEnableIfSequenceChangesSublocation : AppSubcomponent<StandardUIResponseButton>
{
    public override bool Evaluate(StandardUIResponseButton standardUIResponseButton, out Action evalPassAction)
    {
        evalPassAction = null;
        subcomponent.gameObject.SetActive(false);
        
        if (standardUIResponseButton.response == null ||  standardUIResponseButton.response.destinationEntry == null || !standardUIResponseButton.button.interactable) return false;

        var sublocationSequence = standardUIResponseButton.response.destinationEntry.Sequence.Contains("Sublocation");
        
        evalPassAction = () =>
        {
            subcomponent.gameObject.SetActive(sublocationSequence);
        };

        return true;
    }
}
