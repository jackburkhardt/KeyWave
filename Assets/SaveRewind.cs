using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PixelCrushers;
using UnityEngine;
using UnityEngine.UI;

public class SaveRewind : MonoBehaviour
{
    public Image panel;
    public SaveRewindSlot saveSlotTemplate;

    public int saveSlotCount;

    public struct SaveSlot
    {
        public int saveSlotIndex;
        public SaveRewindSlot saveSlotComponent;

        public SaveSlot(int index)
        {
            saveSlotIndex = index;
            saveSlotComponent = null;
        }
    }

    [ItemCanBeNull] private List<SaveSlot> saveSlots;
    
    
   // public List<SaveSystem> saveSlots;
   
    private bool AreSlotsFull => saveSlotCount == 10;

    private int GetFirstAvailableSaveSlotIndex()
    {
        var findIndex = saveSlots.FindIndex(x => x.saveSlotComponent == null);
        if (findIndex == -1)
        {
            var oldestSlot = saveSlots[0];
            for (int i = 0; i < saveSlots.Count; i++)
            {
                if (saveSlots[i].saveSlotComponent.saveTime < oldestSlot.saveSlotComponent.saveTime)
                {
                    oldestSlot = saveSlots[i];
                }
            }
            
            findIndex = oldestSlot.saveSlotIndex;
        }
        return findIndex;
    }
 
    void Start()
    {
        panel.gameObject.SetActive(false);
        
        saveSlotTemplate.gameObject.SetActive(false);

        for (int i = 0; i < 10; i++)
        {
            saveSlots.Add(new SaveSlot(i + 100));
        }
    }

    public void Show()
    {
        panel.gameObject.SetActive(true);
    }

    public void PushSave(string saveName)
    {
        var saveSlotIndex = GetFirstAvailableSaveSlotIndex();
        var saveSlot = saveSlots[saveSlotIndex];
        Destroy(saveSlot.saveSlotComponent.gameObject);
        saveSlot.saveSlotComponent = Instantiate(saveSlotTemplate, panel.transform);
        saveSlot.saveSlotComponent.gameObject.SetActive(true);
        saveSlot.saveSlotComponent.SetFields(saveName, DateTime.Now);
    }
}
