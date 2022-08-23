using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    public class Pool_AnimatedSmoke : PoolSingletonBase<C_SpriteAnimation>
    {
        protected override int MAX_INSTANCES => 50;

        protected override void OnInstanciated(C_SpriteAnimation inst)
        {
            var w = Singleton.Try<Singleton_SDFPhisics_TinyEcs>(s =>
            {
                var w = s.World;

                inst.transform.localScale = Vector3.one * (0.25f + QcMath.SmoothStep(0, 5, GetDistanceToCamera(inst.transform.position)) * 0.75f);

                IEntity cloud = w.CreateEntity("Animated Smoke")

                .AddComponent((ref ParticlePhisics.AffectedByWind dta) => 
                {
                    dta.Buoyancy = (1f + Random.value)*0.2f;// * 1.4f;
                })

                .AddComponent((ref ParticlePhisics.PositionData pos) =>
                {
                    pos.Position = inst.transform.position;
                });

                inst.Restart(cloud);
            });
        }



    }

    [PEGI_Inspector_Override(typeof(Pool_AnimatedSmoke))] internal class Pool_AnimatedSmokeDrawer : PEGI_Inspector_Override { }
}