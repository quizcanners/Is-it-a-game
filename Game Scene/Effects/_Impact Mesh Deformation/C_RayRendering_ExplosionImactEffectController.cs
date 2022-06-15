using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_RayRendering_ExplosionImactEffectController : MonoBehaviour, IPEGI, IPEGI_Handles
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] Transform _shadowRenderer;
        private MaterialPropertyBlock block;

        [NonSerialized] private ShaderProperty.VectorValue _ImpactPosition = new("_ImpactPosition");
        [NonSerialized] private ShaderProperty.FloatValue _ImpactDeformation = new(name: "_ImpactDeformation", 0,1);
        [NonSerialized] private ShaderProperty.FloatValue _ImpactDisintegration = new(name: "_ImpactDisintegration", 0, 1);

        //_ImpactDisintegration

        private LogicWrappers.Request _updatePropertyBlockRequest = new();

      
        private bool _scalingDown;
        private float _targetDeformation = 0;
        private bool _disintegrate;
        private bool _impactPositionSet;

        public bool IsPlaying
        {
            get;
            private set;
        }


        public void Play(Vector3 pos, float impactSize, bool disintegrate) 
        {
            IsPlaying = true;
            _scalingDown = false;
            _targetDeformation = Mathf.Max(impactSize, _targetDeformation);
            Deformation = 0;
            Disintegration = 0;
            bool useHitPosition = disintegrate && _impactPositionSet && (ImpactPosition - pos).magnitude < 2;
             
            if (!useHitPosition)
                ImpactPosition = pos;

            _disintegrate |= disintegrate;
            _impactPositionSet = true;
        }

        public void ResetEffect()
        {
            _disintegrate = false;
            IsPlaying = false;
            Deformation = 0;
            Disintegration = 0;
            _targetDeformation = 0;
            _impactPositionSet = false;
        }

        void OnEnable() => ResetEffect();

        public float Deformation 
        {
            get => _ImpactDeformation.latestValue;
            set 
            {
                _ImpactDeformation.latestValue = value;
                _updatePropertyBlockRequest.CreateRequest();
            }
        }

        public float Disintegration
        {
            get => _ImpactDisintegration.latestValue;
            set
            {
                _ImpactDisintegration.latestValue = value;
                _shadowRenderer.localScale = Vector3.one * 2 * (1-value);
                _updatePropertyBlockRequest.CreateRequest();
            }
        }

    
        protected Vector3 ImpactPosition 
        {
            get => _ImpactPosition.latestValue;
            set 
            {
                _ImpactPosition.latestValue = value;
                _updatePropertyBlockRequest.CreateRequest();
            }
        }

        void LateUpdate() 
        {
            if (IsPlaying) 
            {
                var d = Deformation;

                if (LerpUtils.IsLerpingBySpeed(ref d, _scalingDown ? 0 : _targetDeformation, (_scalingDown ? 4f: 0.75f), unscaledTime: false)) 
                {
                    Deformation =  d ;
                    Disintegration = _disintegrate ? d : 0;  //Mathf.SmoothStep(0, _targetDeformation, d);
                } else 
                {
                    if (_disintegrate)
                        IsPlaying = false;
                    else
                    {
                        if (!_scalingDown)
                            _scalingDown = true;
                        else
                        {
                            IsPlaying = false;
                        }
                    }
                }
            }

            if (_updatePropertyBlockRequest.TryUseRequest()) 
            {
                if (block == null)
                    block = new MaterialPropertyBlock();

                _ImpactPosition.SetOn(block);
                _ImpactDeformation.SetOn(block);
                _ImpactDisintegration.SetOn(block);

                _renderer.SetPropertyBlock(block);
            }
        }

        public void Inspect()
        {
            var changes = pegi.ChangeTrackStart();

            if (IsPlaying)
            {
                var p = IsPlaying;
                "Playing".PegiLabel().ToggleIcon(ref p).Nl().OnChanged(()=> IsPlaying = p);
            }
            else
                "Play Explosion".PegiLabel().Click().Nl().OnChanged(() => Play(transform.position + Vector3.up, Mathf.Max(0.3f, _ImpactDeformation.latestValue), disintegrate: true));


            "Renderer".PegiLabel(70).Edit_IfNull(ref _renderer, gameObject).Nl();
            if (_renderer)
            {
                _ImpactPosition.InspectInList_Nested().Nl();
                _ImpactDeformation.InspectInList_Nested().Nl();
                _ImpactDisintegration.InspectInList_Nested().Nl();
            }

            _updatePropertyBlockRequest.Feed(changes);
        }

        #region Inspector
        public void OnSceneDraw()
        {
            var pos = _ImpactPosition.latestValue;

            pegi.Handle.Position(pos, out var newPos).OnChanged(()=> ImpactPosition = newPos);
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_RayRendering_ExplosionImactEffectController))]
    internal class C_RayRendering_ExplosionImactEffectControllerDrawer : PEGI_Inspector_Override { }
}