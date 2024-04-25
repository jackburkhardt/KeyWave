   using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
   
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemActions : MonoBehaviour, IParticle
    {
        [SerializeField] private ParticleSystemCustomData _customDataSlot;
        
        [Header("Events")]
        [SerializeField] UnityEvent _particleSystemStart, _particleWasBorn, _particleDead, _particleDeadAll = new UnityEvent();
        [SerializeField] private PointToParticle _pointToParticle;
        
        public ParticleSystemCustomData customDataSlot
        {
            get => _customDataSlot;
        }
   
        private ParticleSystem _particleSystem;
   
        private int _uniqueID;
        private List<float> _currentParticlesIds = new List<float>();
        private List<Vector4> _customData = new List<Vector4>();
        private bool _isDead = true;
   
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
   
            if (_particleSystem == null)
            {
                Debug.LogError("Missing particle system!", this);
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

                var color = pointsInfo.Type switch
                {
                    Points.Type.Business => _pointToParticle._particleColorB,
                    Points.Type.Savvy => _pointToParticle._particleColorS,
                    Points.Type.Wellness => _pointToParticle._particleColorW,
                    _ => Color.white
                };
                
                var main = _particleSystem.main;
                main.startColor = color;

    
                // set custom2 to point type
                _particleSystem.GetCustomParticleData(_customData, _customDataSlot);
                for (var i = 0; i < _particleSystem.particleCount; i++)
                {
                    _customData[i] = new Vector4(_customData[i].x, (int)pointsInfo.Type, 0, 0);
                }
                _particleSystem.SetCustomParticleData(_customData, _customDataSlot);
                
                _particleSystem.Play();
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
   
        void UpdateLifeEvents()
        {
            _particleSystem.GetCustomParticleData(_customData, _customDataSlot);
   
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
                    _particleSystemStart?.Invoke();
                }
                _particleWasBorn?.Invoke();
                _currentParticlesIds.Add(_customData[i].x);
   
                if (_uniqueID > _particleSystem.main.maxParticles)
                    _uniqueID = 0;
            }
   
            var ids = _customData.Select(d => d.x).ToList();
            var difference = _currentParticlesIds.Except(ids).Count();
            
   
            for (int i = 0; i < difference; i++)
            {
              _particleDead?.Invoke();
            }
   
            _currentParticlesIds = ids;
            _particleSystem.SetCustomParticleData(_customData, _customDataSlot);
            
            
            if (_particleSystem.particleCount == 0 && !_isDead)
            {
                _particleDeadAll?.Invoke();
                _isDead = true;
            }
        }

        

       
    }
   