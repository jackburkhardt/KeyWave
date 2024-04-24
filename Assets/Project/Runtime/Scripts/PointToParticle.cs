using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(ParticleSystem))]

public class PointToParticle : MonoBehaviour, IParticle
{
    
    public ParticleSystemCustomData customDataSlot { get; }
    [SerializeField] private ParticleSystemCustomData _customDataSlot;

    [HorizontalLine(color: EColor.Blue)][BoxGroup("Business")]
    [Label("Particle Color")] public Color _particleColor0 = Points.Color(Points.Type.Business);
    
    [HorizontalLine(color: EColor.Red)][BoxGroup("Local Savvy")]
    [Label("Particle Color")] public Color _particleColor1 = Points.Color(Points.Type.Savvy);
    
    [HorizontalLine(color: EColor.Green)] [BoxGroup("Wellness")]
    [Label("Particle Color")] public Color _particleColor2 = Points.Color(Points.Type.Wellness);
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }

    
    
    
}
