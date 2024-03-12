using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionsWrapper
{
    
    public List<Action> actions = new();
   
    
    [Serializable]     
    public class Action
    {
        [SerializeField] private string title;
        [SerializeField] private string conversation;
        [SerializeField] private bool available;
        [SerializeField] private string location;
        

        public string Title => title;
        public string Location => location;
        public string Conversation => conversation;
        public bool Available => available;


        public Action(string title, string conversation, bool available, string location)
        {
            this.title = title;
            this.conversation = conversation;
            this.available = available;
            this.location = location;
        }
       
        
        public void SetAvailable(bool available)
        {
            this.available = available;
        }

    }
}

/*

public class ActionsManager : IJsonSerializable, IEventable
{
    [SerializeField]
    private GameObject ActionButton;
    private ActionsWrapper _actionsWrapper;
    // Start is called before the first frame update
    protected override void Awake()
    {
        FileName = "Actions.json";
    }
    protected override void OnLoad()
    {
        Deserialize(ref _actionsWrapper);
        SetLuaVariables();
        CreateButtons(DialogueLua.GetVariable("player_location").asString);
    }


    private void SetLuaVariables()
    {
        for (int i = 0; i < _actionsWrapper.actions.Count; i++)
        {
            var action = _actionsWrapper.actions[i];
            DialogueLua.SetVariable($"action_{i}", action.Available);
        }
    }
    
    public void OnPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
       switch (playerEvent.Type)
       {
           case "conversation_end":
               var playerLocation = DialogueLua.GetVariable("player_location").asString;
               CreateButtons("Lobby");
               break;
           case "move":
               CreateButtons(playerEvent.Value);
               break;
       }
    }

    void SpawnActionButton(ActionsWrapper.Action action, int index)
    {
        var button = Instantiate(ActionButton, transform);
        button.GetComponentInChildren<TMP_Text>().text = action.Title;
        button.GetComponent<DialogueSystemTrigger>().conversation = action.Conversation;
        button.GetComponent<Button>().onClick.AddListener(() => OnActionButtonClick(action, index));
    }

    void SpawnWaitButton()
    {
        var button = Instantiate(ActionButton, transform);
        button.GetComponentInChildren<TMP_Text>().text = "Stay here.";
        button.GetComponent<Button>().onClick.AddListener(() => GameEvent.OnWait(1200000));
    }

    public void CreateButtons(string playerLocation)
    {
        DestroyButtons();
        
       if (DialogueLua.GetVariable("CurrentConversation").asString != string.Empty) return;
        
        for (int i = 0; i < _actionsWrapper.actions.Count; i++)
        {
            var action = _actionsWrapper.actions[i];
            var doesLocationMatch = action.Location == playerLocation;
            var isActionAvailable = DialogueLua.GetVariable($"action_{i}").asBool;
            if (!doesLocationMatch || !isActionAvailable) continue;
            
            var index = i;
            SpawnActionButton(action, index);
        }

        if (transform.childCount == 0)
        {
            SpawnWaitButton();
        }
    }

    IEnumerator CreateButtonsCoroutine()
    {
        yield return new WaitForEndOfFrame();
    }

    void DestroyButtons()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void OnActionButtonClick(ActionsWrapper.Action action, int index)
    {
        action.SetAvailable(false);
        GameEvent.OnAction(index, action.Title);
        DestroyButtons();
    }
 }*/
