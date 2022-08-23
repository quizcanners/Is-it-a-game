using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_SdfBloodController : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer _renderer;

        private MaterialPropertyBlock block;

        private ShaderProperty.FloatValue _Gap = new("_Gap", 0.01f, 5f);
        private ShaderProperty.FloatValue _Size = new("_Size", 0.02f, 0.3f);
        private ShaderProperty.FloatValue _Down = new("_Down", 0.02f, 0.3f);
        private ShaderProperty.FloatValue _RndSeed = new("_RndSeed", 0f, 1f);


        [NonSerialized] public Vector3 PushDirection;

        private float _progress;
        private bool _playing;

        const float MAX_SIZE = 6.5f;


        private float Progress 
        {
            set 
            {
                if (block == null)
                    block = new MaterialPropertyBlock();

                _progress = value;
                _Gap.SetOn(block, Mathf.Lerp(0.61f, 3f, _progress));
                _Size.SetOn(block, Mathf.Lerp(0.055f, 0.02f, _progress));
              
                _RndSeed.SetOn(block);

                var sqRProgress = Mathf.Pow(_progress, 0.75f);

                _Down.SetOn(block, Mathf.Pow(Mathf.Abs(0.25f - sqRProgress) * 2, 2));

                transform.localScale = Vector3.one * Mathf.Lerp(0.25f, MAX_SIZE, _progress);

                _renderer.SetPropertyBlock(block);
            }
        }

        public void Restart() 
        {
           
            _playing = true;
            Progress = 0;
            PushDirection = Vector3.zero;
            _RndSeed.latestValue = UnityEngine.Random.value;
            transform.rotation = Quaternion.identity;
        }

        void Update() 
        {
            if (_playing) 
            {
                if (LerpUtils.IsLerpingBySpeed(ref _progress, 1, 0.75f, unscaledTime: false) || transform.position.y > - 4) 
                {
                    float above = QcMath.SmoothStep(-2f, -0.3f, transform.position.y);

                    transform.position += above * PushDirection * Time.deltaTime;

                    if (_progress > 0.5f) 
                    {
                        transform.position += Vector3.down * (0.2f + above) * (float)(20* Time.deltaTime * (_progress-0.5));
                    }  

                    Progress = _progress;
                } else
                {
                    Pool.Return(this);
                    _playing = false;
                }
            }
        }

        public void Inspect()
        {
            if (block == null)
                block = new MaterialPropertyBlock();

            "Playing".PegiLabel().ToggleIcon(ref _playing).Nl();

            "Renderer".PegiLabel(70).Edit(ref _renderer, gameObject).Nl();
            if (_renderer)
            {
                _Gap.InspectInList_Nested().Nl().OnChanged(() => _Gap.SetOn(_renderer, block));
                _Size.InspectInList_Nested().Nl().OnChanged(() => _Size.SetOn(_renderer, block));

                "Progress".PegiLabel(0.3f).Edit_01(ref _progress).Nl().OnChanged(() => Progress = _progress);
            }

            pegi.Nl();

            "Play".PegiLabel().Click().Nl().OnChanged(()=> 
            {
                gameObject.SetActive(true);
                transform.position = transform.position.Y(0.5f);
                Restart();
            });
        }
    }

    [PEGI_Inspector_Override(typeof(C_SdfBloodController))] internal class C_SdfBloodControllerDrawer : PEGI_Inspector_Override { }
}
