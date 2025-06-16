using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class OnGUIDebug : MonoBehaviour
{
    private string currentText = "";

    private void Update()
    {
        if (DialogueManager.masterDatabase == null) return;
        
        
        
        var points = Points.GetAllPointsTypes();
        
        var sb = new StringBuilder();
        sb.AppendLine("Points:");
        foreach (var point in points)
        {
            sb.AppendLine($"{point.Name}: {DialogueLua.GetItemField( point.Name, "Score").asInt} / {DialogueLua.GetItemField( point.Name, "Max Score").asInt}");
        }
        
        currentText = sb.ToString();
    }


    void OnGUI()
    {
        GUI.Label(
            new Rect(
                5,                   // x, left offset
                Screen.height - 150, // y, bottom offset
                300f,                // width
                150f                 // height
            ),      
            currentText,             // the display text
            GUI.skin.textArea        // use a multi-line text area
        );
    }
}