using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;

[RequireComponent(typeof(ParticleSystem))]

public class PointToParticle : MonoBehaviour
{
    
    private ParticleSystem _particleSystem;
    
   

    [HorizontalLine(color: EColor.Blue)][BoxGroup("Business")]
    [Label("Particle Color")] public Color _particleColorB = Points.Color(Points.Type.Business);
    
    [HorizontalLine(color: EColor.Red)][BoxGroup("Local Savvy")]
    [Label("Particle Color")] public Color _particleColorS = Points.Color(Points.Type.Savvy);
    
    private List<Vector4> _customData = new List<Vector4>();
    
    [HorizontalLine(color: EColor.Green)] [BoxGroup("Wellness")]
    [Label("Particle Color")] public Color _particleColorW = Points.Color(Points.Type.Wellness);

    private void OnEnable()
    {
        GameEvent.OnPlayerEvent += OnPlayerEvent;
    }

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerEvent -= OnPlayerEvent;
    }

    private void OnPlayerEvent(PlayerEvent playerEvent)
    {
        if (playerEvent.EventType == "points")
        {
            Points.PointsField pointsInfo = Points.PointsField.FromString(playerEvent.Data);
            ParticleSystem.Burst pointBurst = _particleSystem.emission.GetBurst(0);
            pointBurst.cycleCount = pointsInfo.Points;
            _particleSystem.emission.SetBurst(0, pointBurst);
            Debug.Log("Set burst to " + pointsInfo.Points + " for " + pointsInfo.Type);

            var color = pointsInfo.Type switch
            {
                Points.Type.Business => _particleColorB,
                Points.Type.Savvy => _particleColorS,
                Points.Type.Wellness => _particleColorW,
                _ => Color.white
            };
                 
            var main = _particleSystem.main;
            main.startColor = color;
            
            _particleSystem.Play();
            
        }
    }
}
