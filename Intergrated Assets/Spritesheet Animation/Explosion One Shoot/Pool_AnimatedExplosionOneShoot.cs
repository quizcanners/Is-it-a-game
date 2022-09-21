using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    public class Pool_AnimatedExplosionOneShoot : PoolSingletonBase<C_SpriteAnimationOneShot>
    {
        protected override int MAX_INSTANCES => 50;

        protected override void OnInstanciated(C_SpriteAnimationOneShot inst)
        {
            inst.transform.localScale = GetScaleBasedOnDistance(inst.transform.position);// Vector3.one * (0.25f + QcMath.SmoothStep(0, 5, GetDistanceToCamera(inst.transform.position)) * 0.75f);

            inst.Restart();
        }



    }

    [PEGI_Inspector_Override(typeof(Pool_AnimatedExplosionOneShoot))] internal class Pool_AnimatedExplosionOneShootDrawer : PEGI_Inspector_Override { }
}