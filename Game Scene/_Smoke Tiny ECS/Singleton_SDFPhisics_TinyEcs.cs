using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using System.Threading.Tasks;
using UnityEngine;
using static QuizCanners.IsItGame.Develop.ParticlePhisics;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_SDFPhisics_TinyEcs : Singleton.BehaniourBase, IPEGI
    {
        private ParticlePhisics _link = new ParticlePhisics();

        private Task currentTask;

     
        private int _started;
        private int _skipped;

        public void Update()
        {
            if (currentTask != null && !currentTask.IsCompleted) 
            {
                _skipped++;
                return;
            }

            _started++;
            currentTask = Task.Run(()=> _link.UpdateSystems(deltaTime: Time.deltaTime));
        }


        void SpawnSmoke() 
        {
            Singleton.Try<Pool_ECS_HeatSmoke>(s =>
            {
                if (s.TrySpawn())
                {
                    var posv3 = Random.insideUnitSphere * 10f;
                    posv3.y = Mathf.Abs(posv3.y);



                    var cloud = _link.GetWorld().CreateEntity("Smoke");
                    cloud.AddComponent<SmokeData>();
                    cloud.AddComponent((ref PositionData pos) =>
                    {
                        pos.Position = posv3;
                    });
                }
            });
        }

        public override void Inspect()
        {
            pegi.Nl();
            "Started: {0} Skipped: {1}".F(_started, _skipped).PegiLabel().Nl();

            pegi.Click(SpawnSmoke).Nl();

        }
    }

    public partial class ParticlePhisics: ITinyECSworld, IPEGI
    {
        public string WorldName => "Battlefield";

        public void Inspect()
        {
            this.GetWorld().Inspect();
            Timer.Nested_Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_SDFPhisics_TinyEcs))] internal class Singleton_SDFPhisics_TinyEcsDrawe : PEGI_Inspector_Override { }
}