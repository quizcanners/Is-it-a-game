using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_SDFPhisics_TinyEcs : Singleton.BehaniourBase, IPEGI
    {
        [SerializeField] private SO_ECSParticlesConfig _config;

        private readonly ParticlePhisics _worldLink = new();

        public World<ParticlePhisics> World => _worldLink.GetWorld();

        public void Update()
        {
            if (_config)
                _worldLink.UpdateSystems(deltaTime: Time.deltaTime, _config);
        }

        public override void Inspect()
        {
            pegi.Nl();
            _worldLink.Nested_Inspect();

            "Settings".PegiLabel().Edit_Inspect(ref _config).Nl();
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