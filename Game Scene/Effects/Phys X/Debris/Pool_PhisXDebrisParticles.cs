using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [DisallowMultipleComponent]
    public class Pool_PhisXDebrisParticles : PoolSingletonBase<C_PhisxDebriParticle>
    {

       

        public void PushFromExplosion(Vector3 origin, float force, float radius) 
        {
            foreach (var el in instances) 
            {
                el.Explosion(origin, force, radius);
            }
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_PhisXDebrisParticles))] internal class Pool_PhisXDebrisParticlesDrawer : PEGI_Inspector_Override { }
}