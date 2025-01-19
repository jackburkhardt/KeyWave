using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Cutscene
{
   public static void Start()
   {
      foreach (var handler in Object.FindObjectsOfType<MonoBehaviour>())
      {
         if (handler is ICutsceneStartHandler startHandler)
         {
            startHandler.OnCutsceneStart();
         }
      }
   }
   
   public static void End()
   {
      foreach (var handler in Object.FindObjectsOfType<MonoBehaviour>())
      {
         if (handler is ICutsceneEndHandler endHandler)
         {
            endHandler.OnCutsceneEnd();
         }
      }
   }
   
}

public interface ICutsceneStartHandler
{
   void OnCutsceneStart();
}

public interface ICutsceneEndHandler
{
   void OnCutsceneEnd();
}


public class SequencerCommandCutsceneStart : SequencerCommand
{
   private void Awake()
   {
      Cutscene.Start();
   }
}


public class SequencerCommandCutsceneEnd : SequencerCommand
{
   private void Awake()
   {
      Cutscene.End();
   }
}
