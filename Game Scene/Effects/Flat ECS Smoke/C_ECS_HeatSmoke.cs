using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using UnityEngine;


namespace QuizCanners.IsItGame.Develop
{
    public class C_ECS_HeatSmoke : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer meshRenderer;
        private World<ParticlePhisics>.Entity _entity;

        #region Inspector
        public void Inspect()
        {
            var changed = pegi.ChangeTrackStart();
            pegi.Nl();
            pegi.Nested_Inspect(ref _entity).Nl();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_ECS_HeatSmoke))] internal class C_ExplosionSmokeDrawer : PEGI_Inspector_Override { }

}