using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPath : MonoBehaviour
{
    public UITextField titleTemplate;
    public UITextField dividerTemplate;
    public Transform pathContainer;
    private string _currentPath;

    private void Awake()
    {
        if (pathContainer == null)
        {
            pathContainer = transform;
        }
    }


    public string[] rootConversationTitles;

    private string _currentConversationTitle;
    
 

    public void InitializePath(string root)
    {
        
        foreach (var child in transform.GetChildren(exclude: (titleTemplate.gameObject.transform, dividerTemplate.gameObject.transform)))
        {
            Destroy(child.gameObject);
        }

        var rootText = Instantiate(titleTemplate.gameObject, pathContainer).GetComponent<UITextField>();
        
        
        rootText.text = root;
        
        _currentPath = root;
    }
    
    /*

    public void OnResponseButtonClick(StandardUIResponseButton responseButton)
    {
        var conversation = responseButton.response.
        
        if (conversation == _currentConversationTitle) return;
        
        foreach (var title in rootConversationTitles)
        {
            if (conversation.Contains(title))
            {
                _currentConversationTitle = conversation;
                InitializePath(title);
                return;
            }
        }
    } */
    
    public void AddToPath(string title)
    {
        var divider = Instantiate(dividerTemplate.gameObject, pathContainer);
        
        var titleText = Instantiate(titleTemplate.gameObject, pathContainer).GetComponent<UITextField>();
        titleText.text = title;
        
        _currentPath += "/" + title;
    }
    
    
    
    
    
    
    
}
