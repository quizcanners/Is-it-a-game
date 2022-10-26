using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.SpecialEffects;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.SpaceEffect
{
    [ExecuteAlways]
    public class Singleton_SpaceAndStars : Singleton.BehaniourBase, ILinkedLerping, IPEGI
    {
        [SerializeField] private SO_SpaceAndStarsConfigs _configurations;
        [SerializeField] private float _parallaxStrength = 1;

        private readonly LinkedLerp.Vector2Value positionLerp = new LinkedLerp.Vector2Value("Position", maxSpeed: 1);
        private readonly LinkedLerp.FloatValue starSizeLerp = new LinkedLerp.FloatValue("Size", initialValue: 0, maxSpeed: 1.5f);

        private readonly LinkedLerp.ShaderColor LIGHT_COLOR = new LinkedLerp.ShaderColor("_Mushroom_Star_Color", initialValue: Color.clear, maxSpeed: 4);
        private readonly LinkedLerp.ShaderColor CLOUDS_COLOR = new LinkedLerp.ShaderColor("_Mushroom_Clouds_Color", initialValue: Color.clear, maxSpeed: 4);
        private readonly LinkedLerp.ShaderColor CLOUDS_COLOR2 = new LinkedLerp.ShaderColor("_Mushroom_Clouds_Color_2", initialValue: Color.clear, maxSpeed: 4);
        private readonly LinkedLerp.ShaderColor BG_COLOR = new LinkedLerp.ShaderColor("_Mushroom_Background_Color", initialValue: Color.clear, maxSpeed: 4);
        private readonly LinkedLerp.ShaderFloat LIGHT_VISIBILITY = new LinkedLerp.ShaderFloat("_Mushroom_Light_Visibility", initialValue: 0, maxSpeed: 2);


        private readonly ShaderProperty.VectorValue LIGHT_POSITION = new ShaderProperty.VectorValue("_Mushroom_Star_Position");
        private readonly ShaderProperty.VectorValue SCROLL_POSITION = new ShaderProperty.VectorValue("_Mushroom_Scroll_Position");
        private readonly ShaderProperty.Feature IS_BLACK_HOLE = new ShaderProperty.Feature("_BLACK_HOLE"); 
        private readonly ShaderProperty.Feature HAS_DYSON_SPHERE = new ShaderProperty.Feature("_DYSON_SPHERE");
        private readonly ShaderProperty.Feature HAS_SPACE_FOG = new ShaderProperty.Feature("_SPACE_FOG");
        private readonly ShaderProperty.Feature HAS_GYROID_FG = new ShaderProperty.Feature("_GYROID_FG");
        //_SPACE_FOG
        [NonSerialized] public float VericalOffset = 0;
        [NonSerialized] public float HorisontalOffset = 0;
        

        // How to Use:  QuizCanners.Utils.Service.Try<SpaceAndStarsController>(s => s.Play("mushrooms.yourkey"));
        public void Play(string key) => _configurations.Play(key);

        #region Animation
        private SpaceAndStarsConfiguration Target => SpaceAndStarsConfiguration.Selected;

        private readonly LerpData _lerpData = new LerpData(unscaledTime: true);

        protected override void OnAfterEnable()
        {
            if (Application.isPlaying)
            {
                _configurations.PlayFirst();
            }
        }

        void Update()
        {
            if (SpaceAndStarsConfiguration.Selected != null)
            {
                _lerpData.Update(this, canSkipLerp: Application.isPlaying == false);
            }
        }

        public void Portion(LerpData ld)
        {
            //VericalOffset
           
            positionLerp.Portion(ld, targetValue: Target.LightSourcePosition);
            starSizeLerp.Portion(ld, targetValue: Target.Size);
          
            CLOUDS_COLOR.Portion(ld, targetValue: Target.CloudsColor);
            CLOUDS_COLOR2.Portion(ld, targetValue: Target.CloudsColor2);
            LIGHT_COLOR.Portion(ld, targetValue: Target.LightColor);
            BG_COLOR.Portion(ld, targetValue: Target.BackgroundColor);

            LIGHT_VISIBILITY.Portion(ld, targetValue: Target.Visibility);
        }

        public void Lerp(LerpData ld, bool canSkipLerp)
        {
            bool starHidden = LIGHT_VISIBILITY.CurrentValue < 0.05f;

            positionLerp.Lerp(ld, canSkipLerp: canSkipLerp || starHidden);
            starSizeLerp.Lerp(ld, canSkipLerp: canSkipLerp || starHidden);
           
            CLOUDS_COLOR.Lerp(ld, canSkipLerp: canSkipLerp || !Target.HasFog);
            CLOUDS_COLOR2.Lerp(ld, canSkipLerp: canSkipLerp || !Target.HasFog);
            LIGHT_COLOR.Lerp(ld, canSkipLerp: canSkipLerp || starHidden);
            BG_COLOR.Lerp(ld, canSkipLerp: canSkipLerp);

            LIGHT_VISIBILITY.Lerp(ld, canSkipLerp: canSkipLerp);


            HAS_GYROID_FG.Enabled = Target.GyroidFog;
            HAS_SPACE_FOG.Enabled = Target.HasFog;
            IS_BLACK_HOLE.Enabled = Target.Type == SpaceAndStarsConfiguration.StarType.BLACK_HOLE;
            HAS_DYSON_SPHERE.Enabled = Target.HasDysonSphere;
            LIGHT_POSITION.GlobalValue = (positionLerp.CurrentValue + new Vector2(HorisontalOffset, VericalOffset) * 0.2f)
                .ToVector4(z: starSizeLerp.CurrentValue, 
                w: _parallaxStrength);
            SCROLL_POSITION.GlobalValue = new Vector2(HorisontalOffset, VericalOffset);
        }
        #endregion

        #region Inspector
        private readonly pegi.EnterExitContext _context = new pegi.EnterExitContext();
        private readonly pegi.EnterExitContext _enteredLerpProp = new pegi.EnterExitContext();

        public override void Inspect()
        {
            pegi.Nl();

            using (_context.StartContext())
            {
                "Configs".PegiLabel().Edit_Enter_Inspect(ref _configurations).Nl();

                if ("Variables & Configs".PegiLabel().IsConditionally_Entered(Application.isEditor).Nl())
                {
                    using (_enteredLerpProp.StartContext())
                    {
                        if (_enteredLerpProp.IsAnyEntered == false)
                            "Parallax Strength".PegiLabel().Edit(ref _parallaxStrength).Nl();
                        
                        LIGHT_COLOR.Enter_Inspect_AsList().Nl();
                        CLOUDS_COLOR.Enter_Inspect_AsList().Nl();
                        LIGHT_POSITION.Enter_Inspect_AsList().Nl();
                    }
                }

                if (_context.IsAnyEntered == false)
                {
                    if (Application.isPlaying && !pegi.PaintingGameViewUI)
                    {
                        "Skip Lerp".PegiLabel().Click(() => this.SkipLerp(_lerpData)).Nl();

                        pegi.Edit(ref VericalOffset, -1, 1).Nl();

                        _lerpData.Nested_Inspect();
                    }

                    QuizCanners.Utils.Singleton.Collector.InspectionWarningIfMissing<Singleton_SpecialEffectShaders>();

                    QuizCanners.Utils.Singleton.Try<Singleton_SpecialEffectShaders>(onFound: s =>
                    {
                        if (!s.NoiseTexture.EnableNoise)
                        {
                            "Noise is disabled".PegiLabel().WriteWarning();
                            "Enable".PegiLabel().Click(() =>
                                s.NoiseTexture.EnableNoise = true);
                        }
                        else
                        {
                            var m = s.NoiseTexture.NeedAttention();
                            if (!m.IsNullOrEmpty())
                                m.PegiLabel().WriteWarning();
                        }
                    }, onFailed: () => "No {0} found".F(nameof(Singleton_SpecialEffectShaders)));
                }
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_SpaceAndStars))] internal class SpaceAndStarsControllerDrawer : PEGI_Inspector_Override { }
}
