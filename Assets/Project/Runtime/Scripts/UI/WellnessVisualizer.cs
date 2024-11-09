using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Audio;
using UnityEngine;
using UnityEngine.Rendering;

public class WellnessVisualizer : MonoBehaviour
{
    int currentWellness => DialogueLua.GetActorField("Daniel Oliviera", "Wellness").asInt;

    private float volumeWeight
    {
        get => GetComponent<Volume>().weight;
        set => GetComponent<Volume>().weight = value;
    } 

    public AnimationCurve weightCurve;
    
    float timeSinceLastFadeEffect = 0;

    private void Awake()
    {
        volumeWeight = 0;
    }

    private int iTime
    {
        get
        {
            timeSinceLastFadeEffect = Time.time;
            return (int)(Time.time - timeSinceLastFadeEffect);
        }
    }

    private float WeightToRange(float weight, float min, float max)
    {
        return (max - min) * weightCurve.Evaluate(weight) + min;
    }
    
    [Button]
    private void SetWellnessTo0()
    {
        DialogueLua.SetActorField("Daniel Oliviera", "Wellness", 0);
    }
    
    [Button]
    private void SetWellnessTo1()
    {
        DialogueLua.SetActorField("Daniel Oliviera", "Wellness", 1);
    }
    
    [Button]
    private void SetWellnessTo2()
    {
        DialogueLua.SetActorField("Daniel Oliviera", "Wellness", 2);
    }
    
    [Button]
    private void SetWellnessTo3()
    {
        DialogueLua.SetActorField("Daniel Oliviera", "Wellness", 3);
    }

    private void Update()
    {

        /*
        switch (currentWellness)
        {
            case 1:
                if (iTime % 30 == 0) DOVirtual.Float(0, 1, 3, volumeWeight => Debug.Log(volumeWeight));
                break;
            case 2:
              //  if (iTime % 15 == 0)LeanTween.easeInOutSine(0, 1, volumeWeight );
                break; 
            case 3:
             //   if (iTime % 5 == 0) LeanTween.easeInOutSine(0, 1, volumeWeight );
                break;
        }
        */
        
      
        
        AudioEngineExtras.SetParameter("Master/HighpassCutoff",WeightToRange(volumeWeight, 10, 40) );
        AudioEngineExtras.SetParameter("Master/HighpassResonance",WeightToRange(volumeWeight, 0, 2) );
        AudioEngineExtras.SetParameter("Master/LowpassCutoff",WeightToRange(volumeWeight, 22000, 100) );
        AudioEngineExtras.SetParameter("Master/LowpassResonance",WeightToRange(volumeWeight, 0, 2) );
        AudioEngineExtras.SetParameter("Master/Distortion",WeightToRange(volumeWeight, 0, 0.39f) );
        
       
        
    }
    
   
}
