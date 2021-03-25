using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{

    [DisallowMultipleComponent]
    [ExecuteAlways]
    public partial class SpecialEffectShadersService : Utils.Singleton.BehaniourBase
    {
        public const string ASSEMBLY_NAME = "Special Effects";

        [SerializeField] public EffectsRandomSessionSeedManager RandomSeed = new EffectsRandomSessionSeedManager();
        [SerializeField] public EffectsTimeManager EffectsTime = new EffectsTimeManager();
        [SerializeField] public GyroscopeParallaxManager GyroscopeParallax = new GyroscopeParallaxManager();
        [SerializeField] public EffectsMousePositionManager MousePosition = new EffectsMousePositionManager();
        [SerializeField] public NoiseTextureManager NoiseTexture = new NoiseTextureManager();

        #region Feeding Events

        public void OnViewChange() 
        {
            EffectsTime.OnViewChange();
        }

        protected void LateUpdate()
        {
            EffectsTime.ManagedLateUpdate();
            GyroscopeParallax.ManagedLateUpdate();
            MousePosition.ManagedLateUpdate();
        }

        private void OnApplicationPause(bool state)
        {
            EffectsTime.OnApplicationPauseManaged(state);
        }

        protected override void AfterEnable()
        {
            RandomSeed.ManagedOnEnable();
            GyroscopeParallax.ManagedOnEnable();
            MousePosition.ManagedOnEnable();
            NoiseTexture.ManagedOnEnable();

            if (!Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.update += LateUpdate;
                #endif
            }
        }

        protected override void OnBeforeOnDisableOrEnterPlayMode()
        {
            if (!Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.update -= LateUpdate;
                #endif
            }
        }


        #endregion

        #region Inspector

        public override string InspectedCategory => Utils.Singleton.Categories.RENDERING;

        [SerializeField] private pegi.EnterExitContext enterExitContext = new pegi.EnterExitContext();

        public override void Inspect()
        {
            pegi.nl();

            using (enterExitContext.StartContext())
            {
                RandomSeed.enter_Inspect_AsList().nl();
                EffectsTime.enter_Inspect_AsList().nl();
                GyroscopeParallax.enter_Inspect_AsList().nl();
                MousePosition.enter_Inspect_AsList().nl();
                NoiseTexture.enter_Inspect_AsList().nl();
            }
        }
        #endregion

    }

    [PEGI_Inspector_Override(typeof(SpecialEffectShadersService))] internal class SpecialEffectShadersServiceDrawer : PEGI_Inspector_Override { }
}