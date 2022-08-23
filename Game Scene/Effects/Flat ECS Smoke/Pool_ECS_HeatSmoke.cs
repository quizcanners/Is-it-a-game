using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using System.Security.Cryptography;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_ECS_HeatSmoke : PoolSingletonBase<C_ECS_HeatSmoke> 
    {
        protected override int MAX_INSTANCES => 500;

        protected override void OnInstanciated(C_ECS_HeatSmoke inst) 
        {
            var w = Singleton.Try<Singleton_SDFPhisics_TinyEcs>(s =>
            {
                var w = s.World;

                IEntity cloud = w.CreateEntity("Smoke")

                .AddComponent<ParticlePhisics.HeatedSmokeData>()

                .AddComponent((ref ParticlePhisics.AffectedByWind dta) => 
                {
                    dta.Buoyancy = 0.5f + Random.value;// * 1.4f;
                })

                .AddComponent((ref ParticlePhisics.PositionData pos) =>
                {
                    pos.Position = inst.transform.position;
                });

                inst.Restart(cloud);
            });
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_ECS_HeatSmoke))] internal class Pool_ExplosionSmokerDrawer : PEGI_Inspector_Override { }
}