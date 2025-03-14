using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PreStartMenuPanel : UIPanel
{
    InputAction clickAction;
    InputAction moveAction;
    InputAction submitAction;
    InputAction cancelAction;


    private void Awake()
    {
        var inputSystemUIInputModule = FindObjectOfType<InputSystemUIInputModule>();
        clickAction = inputSystemUIInputModule.leftClick;
        moveAction = inputSystemUIInputModule.move;
        submitAction = inputSystemUIInputModule.submit;
        cancelAction = inputSystemUIInputModule.cancel;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (clickAction.triggered || moveAction.triggered || submitAction.triggered || cancelAction.triggered)
        {
            Close();
        }
    }
    
}
