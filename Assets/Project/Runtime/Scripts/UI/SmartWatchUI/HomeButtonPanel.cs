using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtonPanel : UIPanel
{

    public Button button;

   public void OnGameSceneStart()
   {
       SmartWatchPanel.onAppOpen += OnAppOpen;
       
   }
   
   public void OnGameSceneEnd()
   {
       Close();
       SmartWatchPanel.onAppOpen -= OnAppOpen;
   }

   public void OnSublocationChange()
   {
       Close();
   }
   
   public void OnAppOpen(SmartWatchAppPanel appPanel)
   {
       if (appPanel.Name == "Home")
       {
           Close();
       }
       else
       {
           Open();
       }
   }
   
   public void OnLinkedConversationStart()
   {
       Close();
   }

   public void OnClick()
   {
       if (button.interactable && button.enabled) button.onClick.Invoke();
   }
    
}
