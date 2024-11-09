using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class ConversationPath : MonoBehaviour
{
    public UITextField titleTemplate;
    public UITextField dividerTemplate;
    public Transform pathContainer;
    private List<PathEntry> _currentPath;

    private void Awake()
    {
        if (pathContainer == null)
        {
            pathContainer = transform;
        }
    }

    private struct PathEntry
    {
        public string Title;
        public DialogueEntry Entry;

        public PathEntry(DialogueEntry entry)
        {
            Entry = entry;
            switch (entry.GetConversation().Title)
            {
                case "Action/Base":
                    Title = "Action";
                    break;
                case "Talk/Base":
                    Title = "Talk";
                    break;
                case "Map":
                    Title = "Map";
                    break;
                default:
                    var subconversation = entry.GetSubconversationQuest();
                    if (subconversation != null)
                    {
                        Title = subconversation.GetCondensedQuestName(characterLimit: 10, append: "...");
                    }
                    else
                    {
                        Title = string.Empty;
                    }
                    break;
            }
        }
    }


    public string[] rootConversationTitles;

    private string _currentConversationTitle;


    public void OnConversationBase(DialogueEntry entry)
    {
        InitializePath(entry);
    }

    public void InitializePath(DialogueEntry entry)
    {
        
        DestroyPath();

        var rootText = Instantiate(titleTemplate.gameObject, pathContainer).GetComponent<UITextField>();
        
        
        _currentPath.Clear();
        _currentPath.Add(new PathEntry(entry));
    }

    private void DestroyPath()
    {
        foreach (var child in transform.GetChildren(exclude: (titleTemplate.gameObject.transform, dividerTemplate.gameObject.transform)))
        {
            Destroy(child.gameObject);
        }
    }
   

    private void InstantiateEntry(PathEntry entry, bool isStart = false)
    {
        var divider = Instantiate(dividerTemplate.gameObject, pathContainer);
        
        var titleText = Instantiate(titleTemplate.gameObject, pathContainer).GetComponent<UITextField>();
        
        titleText.text = entry.Title;
    }

    private void InstantiateFromPath(List<PathEntry> path)
    {
        DestroyPath();
        
        for (int i = 0; i < path.Count; i++)
        {
            InstantiateEntry(path[i], i == 0);
        }
    }

    private void SetExistingPathEntryAsEnd(PathEntry entry)
    {
        if (!_currentPath.Contains(entry)) return;
        
        var index = _currentPath.IndexOf(entry);
        
        
        
        for (int i = index + 1; i < _currentPath.Count; i++)
        {
            _currentPath.Remove(_currentPath[i]);
        }
        
        InstantiateFromPath(_currentPath);
    }
    
    
    
    
    
    
    
}
