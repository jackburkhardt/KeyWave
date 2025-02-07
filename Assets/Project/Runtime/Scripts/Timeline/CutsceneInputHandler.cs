using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class CutsceneInputHandler : MonoBehaviour, ICutsceneStartHandler, ICutsceneEndHandler
{
    public enum InputType
    {
        KeyDown,
    }
    
    public InputType inputType;
    
    [ShowIf("inputType", InputType.KeyDown)]
    public KeyCode keyCode;
    
    [ReadOnly] [SerializeField] private bool isCutscenePlaying;

    public UnityEvent OnInput;
    
    [Label("On Cutscene Start")]
    public UnityEvent OnCutsceneStartEvent;
    [Label("On Cutscene End")]
    public UnityEvent OnCutsceneEndEvent;

    private void Update()
    {
        if (!isCutscenePlaying) return;
        
        switch (inputType)
        {
            case InputType.KeyDown:
                if (Input.GetKeyDown(keyCode))
                {
                    OnInput.Invoke();
                }
                break;
        }
    }

    public void OnCutsceneStart()
    {
        isCutscenePlaying = true;
        OnCutsceneStartEvent.Invoke();
    }

    public void OnCutsceneEnd()
    {
       isCutscenePlaying = false;
         OnCutsceneEndEvent.Invoke();
    }
}
