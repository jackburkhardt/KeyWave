using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.UI.Particles
{
    [RequireComponent(typeof(ParticleSystem))]
    
    [Serializable]
    public class ParticleEvent : UnityEvent<ParticleSystem.Particle> {}
    [Serializable]
    public class ParticleSystemEvent : UnityEvent<ParticleSystem> {}
    public class ParticleSystemActions : MonoBehaviour, IParticle
    {
        [SerializeField] private ParticleSystemCustomData _customDataSlot;
        [SerializeField] private ParticleSystemCustomData _customDataToInvoke;

        [Header("Events")]
        [SerializeField] ParticleSystemEvent _particleSystemStart = new ParticleSystemEvent();

        [SerializeField] ParticleSystemEvent _particleDeadAll = new ParticleSystemEvent();
        [SerializeField] ParticleEvent _particleWasBorn, _particleDead = new ParticleEvent();

        private List<float> _currentParticlesIds = new List<float>();
        private List<Vector4> _customData = new List<Vector4>();
        private bool _isDead = true;
        private ParticleSystem.Particle[] _oldParticles = new ParticleSystem.Particle[]{ };
        [SerializeField] private IParticle _particleCustomDataComponent;
        private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[]{ };

        private ParticleSystem _particleSystem;


        private int _uniqueID;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
   
            if (_particleSystem == null)
            {
                Debug.LogError("Missing particle system!", this);
                
            }
        }

        private void LateUpdate()
        {
            if (_particleSystem == null)
            {
                return;
            }
       
            UpdateLifeEvents();
        }

        public ParticleSystemCustomData customDataSlot
        {
            get => _customDataSlot;
        }

        void UpdateLifeEvents()
        {
            _particleSystem.GetCustomParticleData(_customData, _customDataSlot);
            
            _particles = new ParticleSystem.Particle[_particleSystem.particleCount];
            
            _particleSystem.GetParticles(_particles);
           
            for (var i = 0; i < _particleSystem.particleCount; i++)
            {
                if (_customData[i].x != 0.0f)
                {
                    continue;
                }
           
                _customData[i] = new Vector4(++_uniqueID, 0, 0, 0);

                if (_isDead)
                {
                    _isDead = false;
                    _particleSystemStart?.Invoke(_particleSystem);
                }
                _particleWasBorn?.Invoke(_particles[i]);
                _currentParticlesIds.Add(_customData[i].x);
   
                if (_uniqueID > _particleSystem.main.maxParticles)
                    _uniqueID = 0;
            }
   
            var ids = _customData.Select(d => d.x).ToList();
            var difference = _currentParticlesIds.Except(ids).ToList();

            var nonMatchingParticles = _oldParticles.Where(p => !_particles.Contains(p)).ToList();

            for (int i = 0; i < difference.Count; i++)
            {
                var deadParticleIndex = _currentParticlesIds.IndexOf(difference[i]);
                var deadParticle = _oldParticles[deadParticleIndex];
                _particleDead?.Invoke(deadParticle);
                
            }
          
           
            
            _currentParticlesIds = ids;
            _particleSystem.SetCustomParticleData(_customData, _customDataSlot);
            
            
            if (_particleSystem.particleCount == 0 && !_isDead)
            {
                _particleDeadAll?.Invoke(_particleSystem);
                _isDead = true;
            }
            
            _oldParticles = _particles;
        }
    }
}