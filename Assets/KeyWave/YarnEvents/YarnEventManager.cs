using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System.Linq;

namespace YarnEvents
{
    public class YarnEventManager : MonoBehaviour
    {

        

        private string _yarnEventPath;
        public static List<YarnEvent> ChapterYarnEvents = new List<YarnEvent>();


        // Start is called before the first frame update
        private void Awake()
        {
            _yarnEventPath = Application.streamingAssetsPath + "/GameData/YarnEvents/";
            GameEvent.OnGameLoad += Load;
            GameEvent.OnYarnEventActive += NewYarnEvent;
        }

        private void Load()
        {
            ChapterYarnEvents =
DataManager.DeserializeData<List<YarnEvent>>($"{_yarnEventPath}{GameManager.currentModule}/{GameManager.currentChapter}.json");
            Debug.Log(ChapterYarnEvents.Count + " YarnEvents loaded.");
        }

        //class that ties a dialoguerunner with a yarnevent
        private void NewYarnEvent(YarnEvent yarnEvent)
        {
            YarnEventInstance yarnEventInstance = new YarnEventInstance(yarnEvent);
        }
    }

    public class YarnEventInstance
    {
        DialogueRunner DialogueRunner;
        YarnEvent YarnEvent;
        
        public YarnEventInstance(YarnEvent yarnEvent)
        {
            YarnEvent = yarnEvent;
            DialogueRunner = (GameObject.Instantiate(Resources.Load($"Yarn/{GameManager.currentModule}/Dialogue System") as GameObject)).GetComponent<DialogueRunner>();
            DialogueRunner.name = YarnEvent.Name; // debug purposes
            if (DialogueRunner.yarnProject == null) DialogueRunner.SetProject(Resources.Load($"Yarn/{GameManager.currentModule}/YarnProject") as YarnProject);
            DialogueRunner.onDialogueComplete.AddListener(OnRunnerEnd);
            DialogueRunner.onNodeComplete.AddListener(OnNodeEnd);
            DialogueRunner.onNodeStart.AddListener(OnNodeStart);

            //queue yarnevent activation for when appropriate
          //  GameEvent.OnAnyEvent += StartYarn; 

        }

        private void OnNodeEnd(string node) => GameEvent.YarnNodeEnd(node);

        private void OnNodeStart(string node) => GameEvent.YarnNodeStart(node);

        private void StartYarn()
        {

            if (!GameManager.isControlEnabled) return;
            Debug.Log("starting");
          //  GameEvent.OnAnyEvent -= StartYarn;
            GameEvent.InteractionStart(DialogueRunner.GetComponent<Interaction.Dialogue>()); // otherwise "player_interact" can get confused
            DialogueRunner.LoadStateFromPlayerPrefs(); //load variables, otherwise variables don't save
            DialogueRunner.StartDialogue(YarnEvent.Node);
        }



        private void OnRunnerEnd()
        {
            GameEvent.EndYarnEvent(YarnEvent);
            YarnEvent.State = YarnEvent.YarnEventState.Inactive;

            bool CompletionIsFulfilled = YarnEvent.CompletionCriteria.All(criterion => criterion.Fulfilled);
            bool ActivationHistoryRecorded = YarnEvent.ActivationCriteria.All(criterion => criterion.History);

            if (CompletionIsFulfilled && (ActivationHistoryRecorded || YarnEvent.CompletionCriteria.Count > 0))
                YarnEvent.State = YarnEvent.YarnEventState.Completed; 

            Debug.Log("'" + YarnEvent.Name + "' is " + YarnEvent.State);
            DialogueRunner.SaveStateToPlayerPrefs();
            GameObject.Destroy(DialogueRunner.gameObject);
        }

     
        


    }

}
