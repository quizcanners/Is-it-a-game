using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;

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

                World<ParticlePhisics>.Entity cloud = w.CreateEntity("Smoke");

                cloud.AddComponent((ref ParticlePhisics.SmokeData smoke) => 
                {
                    smoke.Temperature = 10;
                });
                cloud.AddComponent((ref ParticlePhisics.PositionData pos) =>
                {
                    pos.Position = inst.transform.position;
                });

                inst.Restart(cloud);
            });
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_ECS_HeatSmoke))] internal class Pool_ExplosionSmokerDrawer : PEGI_Inspector_Override { }
}