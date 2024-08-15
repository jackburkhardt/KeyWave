using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using PixelCrushers;
using UnityEngine;

public class SaveRewindSlot : MonoBehaviour
{
    public UITextField saveNameText, saveTimeText, saveSlotText;
    public DateTime saveTime;
    
    public void SetFields(string saveName, DateTime saveTime)
    {
        saveNameText.text = saveName;
        saveTimeText.text = saveTime.ToString(CultureInfo.InvariantCulture);
    }
    
}
