using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Runtime.Scripts.UI.Particles
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PointToParticle : MonoBehaviour
    {
        [HorizontalLine(color: EColor.Blue)][BoxGroup("Business")]
        [Label("Particle Color")] public Color _particleColorB = Points.Color(Points.Type.Business);

        [HorizontalLine(color: EColor.Red)][BoxGroup("Local Savvy")]
        [Label("Particle Color")] public Color _particleColorS = Points.Color(Points.Type.Savvy);

        [HorizontalLine(color: EColor.Green)] [BoxGroup("Wellness")]
        [Label("Particle Color")] public Color _particleColorW = Points.Color(Points.Type.Wellness);

        private List<Vector4> _customData = new List<Vector4>();
        private bool done;

        private ParticleSystem _particleSystem;
        private Queue<Points.PointsField> _particleQueue = new Queue<Points.PointsField>();

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            GameEvent.OnPlayerEvent += OnPlayerEvent;
        }

        private void OnDisable()
        {
            GameEvent.OnPlayerEvent -= OnPlayerEvent;
        }

        private void Update()
        {
            if (!_particleSystem.IsAlive() && _particleQueue.Count > 0)
            {
                var pointsInfo = _particleQueue.Dequeue();
                ParticleSystem.Burst pointBurst = _particleSystem.emission.GetBurst(0);
                pointBurst.cycleCount = pointsInfo.Points;
                _particleSystem.emission.SetBurst(0, pointBurst);
            
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

        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            if (playerEvent.EventType == "points")
            {
                Points.PointsField pointsInfo = Points.PointsField.FromString(playerEvent.Data);
                _particleQueue.Enqueue(pointsInfo);
            }
        }
    }
}