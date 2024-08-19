using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PixelCrushers;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class SaveRewind : MonoBehaviour
{
    public Image panelContainer;
    public Image saveSlotTemplateContainer;
    public SaveRewindSlot saveSlotTemplate;

 
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

    [ItemCanBeNull] private static List<SaveSlot> saveSlots;
    
    
   // public List<SaveSystem> saveSlots;
   
   
    private int GetFirstAvailableSaveSlotIndex()
    {
        var findIndex = saveSlots.FindIndex(x => x.saveSlotComponent == null);
        if (findIndex == -1)
        {
            Debug.Log("No available save slots, finding oldest save slot");
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
        saveSlotTemplate.gameObject.SetActive(false);
        saveSlots = new List<SaveSlot>();
        
        Hide();

        for (int i = 0; i < 100; i++)
        {
            saveSlots.Add(new SaveSlot(i + 100));
        }
    }

    public void Show()
    {
        panelContainer.gameObject.SetActive(true);
        RefreshLayoutGroups.Refresh(panelContainer.gameObject);
        Time.timeScale = 0;
        
    }

    public void Hide()
    {
        Time.timeScale = 1;
        panelContainer.gameObject.SetActive(false);
    }

    public void PushSave(string saveName)
    {
        var saveSlotIndex = GetFirstAvailableSaveSlotIndex();
        var saveSlot = saveSlots[saveSlotIndex];
        if (saveSlot.saveSlotComponent != null) Destroy(saveSlot.saveSlotComponent.gameObject);
        saveSlot.saveSlotComponent = Instantiate(saveSlotTemplate, saveSlotTemplateContainer.transform);
        saveSlot.saveSlotComponent.gameObject.SetActive(true);
        saveSlot.saveSlotComponent.transform.SetSiblingIndex(1);
        saveSlot.saveSlotComponent.SetFields(saveName, DateTime.Now, saveSlot.saveSlotIndex);
        
        saveSlots[saveSlotIndex] = saveSlot;
        SaveSystem.SaveToSlot(saveSlot.saveSlotIndex);
        Debug.Log("Pushed save to slot " + saveSlot.saveSlotIndex);
        
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (panelContainer.gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
    
}
