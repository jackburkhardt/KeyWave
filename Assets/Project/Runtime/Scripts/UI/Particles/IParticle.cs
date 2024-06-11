using UnityEngine;

namespace Project.Runtime.Scripts.UI.Particles
{
    public interface IParticle
    {
        public ParticleSystemCustomData customDataSlot { get; }
    }
}