using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    [ExecuteAlways]
    public class Pool_MuzzleFlashes : PoolSingletonBase<C_ParticleSystemBurst>
    {


        public override void Inspect()
        {
            base.Inspect();
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_MuzzleFlashes))] 
    internal class Pool_MuzzleFlashesDrawer : PEGI_Inspector_Override { }
}
