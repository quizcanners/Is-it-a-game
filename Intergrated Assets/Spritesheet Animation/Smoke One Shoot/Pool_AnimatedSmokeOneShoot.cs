using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    public class Pool_AnimatedSmokeOneShoot : PoolSingletonBase<C_SpriteAnimationSmokeOneShot>
    {
        protected override int MAX_INSTANCES => 200;

        protected override void OnInstanciated(C_SpriteAnimationSmokeOneShot inst)
        {
            inst.transform.localScale = Vector3.one * (0.25f + QcMath.SmoothStep(0, 5, GetDistanceToCamera(inst.transform.position)) * 0.75f);

            inst.Restart();
        }



    }

    [PEGI_Inspector_Override(typeof(Pool_AnimatedSmokeOneShoot))] internal class Pool_AnimatedSmokeOneShootDrawer : PEGI_Inspector_Override { }
}