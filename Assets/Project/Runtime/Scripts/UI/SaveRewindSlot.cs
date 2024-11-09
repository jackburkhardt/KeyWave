using System;
using System.Globalization;
using PixelCrushers;
using UnityEngine;

public class SaveRewindSlot : MonoBehaviour
{
    public UITextField saveNameText, saveTimeText, saveSlotText;
    public DateTime saveTime;
    private int saveSlotIndex;
    
    public void SetFields(string saveName, DateTime saveTime, int saveSlotIndex)
    {
        saveNameText.text = saveName;
        saveTimeText.text = saveTime.ToString(CultureInfo.InvariantCulture);
        this.saveSlotIndex = saveSlotIndex;
    }

    private void OnEnable()
    {
        saveSlotText.text = transform.GetSiblingIndex().ToString();
    }

    public void LoadSlot()
    {
        Debug.Log("Loading save slot " + saveSlotIndex);
        SaveSystem.LoadFromSlot(saveSlotIndex);
        SaveRewindAutosave.autosaveEnabled = false;
    }
}
