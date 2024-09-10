using System;
using System.Collections;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Syntax: AudioFade(audioSource, duration)
    /// </summary>
    public class SequencerCommandEnterSublocation : SequencerCommand
    {
        const bool debug = true;
        private IEnumerator Start()
        {
            string location = GetParameter(0);
            string sublocation = GetParameter(1);
            float duration = GetParameterAsFloat(2, 3);
            bool always = GetParameterAsBool(3, false);

            if (!always && DialogueLua.GetLocationField(location, "Current Sublocation").asString == sublocation)
            {
                if (debug) Debug.Log("Already in sublocation");
                yield break;
            }
            
            var currentSublocation = DialogueLua.GetLocationField(location, "Current Sublocation").asString;
            
            var locationScene = SceneManager.GetSceneByName(location);
            
            var destinationSublocationGameObject = locationScene.FindGameObject(sublocation);
            
            
            yield return null;
            if (destinationSublocationGameObject == null)
            {
                if (debug) Debug.Log("Sublocation not found");
                yield break;
            }
            Sequencer.PlaySequence("SetMenuPanelTrigger(1, false)");
            
            Sequencer.PlaySequence($"Fade(stay, {duration/4})");

            yield return new WaitForSeconds(duration/2);
            
           Sequencer.PlaySequence("ClearPanel()");
           
            
            if (currentSublocation != String.Empty)
            {
                var currentSublocationGameObject = locationScene.FindGameObject(currentSublocation == String.Empty ? location : currentSublocation);
                if (currentSublocationGameObject != null) currentSublocationGameObject.SetActive(false);
            }
            
            
            
            destinationSublocationGameObject.SetActive(true);
            DialogueLua.SetLocationField(location, "Current Sublocation", sublocation);
            
            Sequencer.PlaySequence($"Fade(unstay, {duration/4})");
            
            yield return new WaitForSeconds(duration/4);
        }
    }
    
    public class SequencerCommandExitSublocation : SequencerCommand
    {
        private IEnumerator Start()
        {
            string location = GetParameter(0);
            var sublocation = DialogueLua.GetLocationField(location, "Current Sublocation").asString;
            float duration = GetParameterAsFloat(1, 2);

            var locationScene = SceneManager.GetSceneByName(location);
            
            var sublocationGameObject = locationScene.FindGameObject(sublocation);
            
            if (sublocationGameObject == null) yield break;
            
            Sequencer.PlaySequence("SetMenuPanelTrigger(1, false)");
            
            Sequencer.PlaySequence($"Fade(stay, {duration/4})");

            yield return new WaitForSeconds(duration/2);
            
            Sequencer.PlaySequence("ClearPanel()");
            
            sublocationGameObject.SetActive(false); 
            DialogueLua.SetLocationField(location, "Current Sublocation", string.Empty);
            
            var locationGameObject = locationScene.FindGameObject(location);
            if (locationGameObject != null) locationGameObject.SetActive(true);
            
            
            Sequencer.PlaySequence($"Fade(unstay, {duration/4})");
            
            yield return new WaitForSeconds(duration/4);
            
        }
    }
    
}