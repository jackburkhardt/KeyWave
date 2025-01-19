using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

public class InstantiateGraphic : MonoBehaviour
{
    public RectTransform graphicTemplateHolder;

    public Graphic graphicTemplate;
    
    private GameObject _graphicInstance;
    
    public enum Action
    {
        None,
        CopyGraphicColorToTemplateInstance
    }
    
    public Action action;
    
    
    [ShowIf( "action", Action.CopyGraphicColorToTemplateInstance)]
    public Graphic graphic;
    
    public bool destroyInstanceOnDisable = true;
    
    

    public void OnEnable()
    {
        graphicTemplate.gameObject.SetActive(false);
        
        _graphicInstance = Instantiate(graphicTemplate, graphicTemplateHolder.transform).gameObject;
        _graphicInstance.gameObject.SetActive(true);
        _graphicInstance.SendMessage("OnInstantiate", this.transform, SendMessageOptions.DontRequireReceiver);

        if (action ==Action.CopyGraphicColorToTemplateInstance) _graphicInstance.GetComponent<Graphic>().color = GetComponent<Graphic>().color;
    }
    
    public void OnDisable()
    {
        Destroy(_graphicInstance);
    }

    public void OnValidate()
    {
        graphic ??= GetComponent<Graphic>();
    }

    public void OnDraggableInterfaceViewportExit()
    {
        _graphicInstance.SetActive(true);
    }
    
    public void OnDraggableInterfaceViewportEnter()
    {
        _graphicInstance.SetActive(false);
    }
    
  
}
