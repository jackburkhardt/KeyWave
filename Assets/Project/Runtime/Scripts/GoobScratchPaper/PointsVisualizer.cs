using System.Collections;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class PointsVisualizer : MonoBehaviour
{
   
    public bool pauseDialogueSystem = true;
    
    [ShowIf("pauseDialogueSystem")]
    [SerializeField] private float dialogueSystemPauseDuration = 2f;
    
    [SerializeField] private Animator animator;
    
    public string animationTrigger = "Points";
    
    private bool _isAnimating;


    private void OnEnable()
    {
        Points.OnPointsChange += VisualizePointType;
    }

    private void OnDisable()
    {
        Points.OnPointsChange -= VisualizePointType;
    }


    private void VisualizePointType(Points.Type type, int amount)
    {
        if (amount == 0) return;
        if (pauseDialogueSystem) DialogueManager.instance.Pause();
        
        _isAnimating = true;
        
        StartCoroutine(Animate(type));
        
        IEnumerator Animate(Points.Type type)
        {
            yield return new WaitForEndOfFrame();
        
            animator.SetTrigger(animationTrigger);
        
            yield return new WaitForSeconds(dialogueSystemPauseDuration);
            if (pauseDialogueSystem) DialogueManager.instance.Unpause();
        }
        
        
    }
    
    
}
