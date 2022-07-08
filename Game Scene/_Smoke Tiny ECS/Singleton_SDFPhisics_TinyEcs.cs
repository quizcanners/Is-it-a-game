using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using System.Threading.Tasks;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_SDFPhisics_TinyEcs : Singleton.BehaniourBase, IPEGI
    {
        private ParticlePhisics _worldLink = new ParticlePhisics();

        private Task currentTask;

     
        private int _started;
        private int _skipped;

        public World<ParticlePhisics> World => _worldLink.GetWorld();

        public void Update()
        {
            if (currentTask != null && !currentTask.IsCompleted) 
            {
                _skipped++;
                return;
            }

            _started++;

            // currentTask = Task.Run(()=> 

            _worldLink.UpdateSystems(deltaTime: Time.deltaTime);
            
            //);
        }

        public override void Inspect()
        {
            pegi.Nl();
            "Started: {0} Skipped: {1}".F(_started, _skipped).PegiLabel().Nl();
            _worldLink.Nested_Inspect();
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