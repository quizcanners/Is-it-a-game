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

        [NonSerialized] private Transform impactTracker;

        //_ImpactDisintegration

        private LogicWrappers.Request _updatePropertyBlockRequest = new();

      
        private bool _scalingDown;
        private float _targetDeformation = 0;
        private bool _disintegrate;

        public bool IsPlaying
        {
            get;
            private set;
        }

        

        public void Play(Vector3 pos, float impactSize, bool disintegrate, Transform origin = null) 
        {
            IsPlaying = true;
            _scalingDown = false;
            _targetDeformation = Mathf.Max(impactSize, _targetDeformation);
            if (disintegrate)
            {
                Deformation = 0;
                Disintegration = 0;
            }
            //bool useHitPosition = disintegrate && _impactPositionSet && (ImpactPosition - pos).magnitude < 2;
             
           // if (!useHitPosition)
            ImpactPosition = pos;

            _disintegrate |= disintegrate;

            if (origin) 
            {
                if (!impactTracker)
                {
                    impactTracker = new GameObject("Impact Tracker").transform;
                }

                impactTracker.parent = origin.transform;
                impactTracker.position = pos;
            }

            ManagedUpdate();
        }

        public void ResetEffect()
        {
            _disintegrate = false;
            IsPlaying = false;
            Deformation = 0;
            Disintegration = 0;
            _targetDeformation = 0;
        }

        void OnEnable() => ResetEffect();

        void OnDisable() 
        {
            if (impactTracker)
                impactTracker.gameObject.DestroyWhatever();
        }

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
                if (_shadowRenderer)
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
            ManagedUpdate();
        }



        void ManagedUpdate() 
        {
          
            if (IsPlaying)
            {
                var d = Deformation;

                if (impactTracker)
                {
                    ImpactPosition = impactTracker.transform.position;
                }

                float UpscaleSpeed = _disintegrate ? 1f : 10f;

                if (LerpUtils.IsLerpingBySpeed(ref d, _scalingDown ? 0 : _targetDeformation, (_scalingDown ? 1f : UpscaleSpeed), unscaledTime: false))
                {
                    Deformation = d;
                    Disintegration = _disintegrate ? d * d : 0;  //Mathf.SmoothStep(0, _targetDeformation, d);
                }
                else
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

        #region Inspector
        public void Inspect()
        {
            var changes = pegi.ChangeTrackStart();

            if (impactTracker)
            {
                "Impac Tracker".PegiLabel().Edit(ref impactTracker).Nl();
            }

            if (IsPlaying)
            {
                var p = IsPlaying;
                "Playing".PegiLabel().ToggleIcon(ref p).Nl().OnChanged(() => IsPlaying = p);
            }
            else
            {
                "Play Explosion".PegiLabel().Click().Nl().OnChanged(() => Play(transform.position + Vector3.up, Mathf.Max(0.3f, _ImpactDeformation.latestValue), disintegrate: true));
                "Play Impact".PegiLabel().Click().Nl().OnChanged(() => Play(ImpactPosition, 1, disintegrate: false));
            }

            "Renderer".PegiLabel(70).Edit_IfNull(ref _renderer, gameObject).Nl();
            if (_renderer)
            {
                _ImpactPosition.InspectInList_Nested().Nl();
                _ImpactDeformation.InspectInList_Nested().Nl();
                _ImpactDisintegration.InspectInList_Nested().Nl();

                var both = Deformation;
                if ("Together".PegiLabel(60).Edit(ref both, 0, 1).Nl()) 
                {
                    Deformation = both;
                    Disintegration = both * both;
                }

            }

            _updatePropertyBlockRequest.Feed(changes);
        }

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