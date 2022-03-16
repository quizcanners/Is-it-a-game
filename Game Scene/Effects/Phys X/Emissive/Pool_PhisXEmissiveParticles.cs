using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [DisallowMultipleComponent]
    public class Pool_PhisXEmissiveParticles : PoolSingletonBase<C_PhisxEmissiveParticle>
    {

        public void PushFromExplosion(Vector3 origin, float force, float radius) 
        {
            foreach (var el in instances) 
            {
                el.Explosion(origin, force, radius);
            }
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_PhisXEmissiveParticles))] internal class Pool_PhisXEmissiveParticlesDrawer : PEGI_Inspector_Override { }
}