using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SaveRewindAutosave : MonoBehaviour
{
    public static bool autosaveEnabled = true;
    
    public SaveRewind saveRewind;
    // Start is called before the first frame update
    private void OnConversationLine(Subtitle subtitle)
    {
        if (!autosaveEnabled)
        {
            autosaveEnabled = true;
            return;
        }
        
        if (subtitle.formattedText.text == string.Empty) return;
        if (subtitle.dialogueEntry.IsResponseChild()) return;
        
        var saveName = subtitle.speakerInfo.Name.Length != 0 ? $"{subtitle.speakerInfo.Name}: {subtitle.formattedText.text}" : subtitle.formattedText.text;
        saveRewind.PushSave(saveName);
    }
}
