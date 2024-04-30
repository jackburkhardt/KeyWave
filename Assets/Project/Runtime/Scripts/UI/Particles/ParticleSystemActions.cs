   using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
   
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemActions : MonoBehaviour
    {
        [SerializeField] private PointToParticle _pointToParticle;
   
        private ParticleSystem _particleSystem;
   
        private int _uniqueID;
   
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
   
            if (_particleSystem == null)
            {
                Debug.LogError("Missing particle system!", this);
                return;
            }

            GameEvent.OnPlayerEvent += OnPlayerEvent;
        }
        
        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            if (playerEvent.EventType == "points")
            {
                Points.PointsField pointsInfo = (Points.PointsField)playerEvent.Data;
                ParticleSystem.Burst pointBurst = _particleSystem.emission.GetBurst(0);
                pointBurst.cycleCount = pointsInfo.Points;
                _particleSystem.emission.SetBurst(0, pointBurst);
                Debug.Log("Set burst to " + pointsInfo.Points + " for " + pointsInfo.Type);

                var color = pointsInfo.Type switch
                {
                    Points.Type.Business => _pointToParticle._particleColorB,
                    Points.Type.Savvy => _pointToParticle._particleColorS,
                    Points.Type.Wellness => _pointToParticle._particleColorW,
                    _ => Color.white
                };
                
                var main = _particleSystem.main;
                main.startColor = color;
                
                _particleSystem.Play();
            }
        }
       
    }
   