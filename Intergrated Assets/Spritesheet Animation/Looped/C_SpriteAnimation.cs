using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_SpriteAnimation : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _introMaterial;
        [SerializeField] private Material _outroMaterial;
        [SerializeField] private float _speed = 1;

        private IEntity _entity;

        ShaderProperty.FloatValue _animation = new ShaderProperty.FloatValue("_Frame");
        ShaderProperty.FloatValue _loopVisibility = new ShaderProperty.FloatValue("_LoopVisibility");
        ShaderProperty.FloatValue _emission = new ShaderProperty.FloatValue("_Emission");
        MaterialPropertyBlock block;

        LogicWrappers.Request blockDirty = new LogicWrappers.Request();

        private bool isAnimating = true;
        private bool _isIntro;
        private float _randZ;


        public float Health;

        public void UpscaleBy(float upscale) => transform.localScale *= upscale;

        private bool IsIntro 
        {
            get => _isIntro;
            set 
            {
                _isIntro = value;
                _meshRenderer.material = _isIntro ? _introMaterial : _outroMaterial;
            }
        }

        private float Animation 
        {
            get => _animation.latestValue;
            set
            {
                _animation.latestValue = value; //SetOn(block, value);
                blockDirty.CreateRequest();
            }
        }

        private float IsLooping
        {
            get => _loopVisibility.latestValue;
            set
            {
                _loopVisibility.latestValue = value; //SetOn(block, value);
                blockDirty.CreateRequest();
            }
        }

    

        void Awake()
        {
            _randZ = Random.value * 360;
        }

        void LateUpdate() 
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.Z(_randZ));

            if (isAnimating)
            {
                Animation += Time.deltaTime * _speed;

                if (_entity!= null)
                    transform.position = _entity.GetComponent<ParticlePhisics.PositionData>().Position;

                if (IsIntro)
                {
                    IsLooping = Mathf.Max(IsLooping, QcMath.SmoothStep(0.4f, 1f, Animation)); 

                    Health -= Time.deltaTime / (0.01f + Pool.VacancyFraction<C_SpriteAnimation>()) ;

                    _emission.latestValue = QcMath.SmoothStep(1f, 0.75f, IsLooping);

                    if (Health <= 0)
                    {
                        Health = 0;

                        if (Animation >= 1)
                        {
                            IsIntro = false;  
                        }

                    }

                    Animation %= 1;
                } else 
                {

                    IsLooping = Mathf.Min(IsLooping, QcMath.SmoothStep(0.8f, 0f, Animation));

                    if (Animation >= 1) 
                    {
                        Animation = 1;

                        if (_entity != null)
                            _entity.Destroy();

                        Pool.Return(this);
                        // Destroy
                    }
                }
            }

            if (blockDirty.TryUseRequest())
            {
                if (block == null)
                    block = new MaterialPropertyBlock();

                _animation.SetLatestValueOn(block);
                _loopVisibility.SetLatestValueOn(block);
                _emission.SetLatestValueOn(block);

                _meshRenderer.SetPropertyBlock(block);
            }
        }

        public void Restart(IEntity entity)
        {
            _entity = entity;
            Restart(1);
        }

        void Restart(float health) 
        {
            Health = health;
            IsLooping = 0;
            Animation = 0;
            IsIntro = true;
            isAnimating = true;
           
           

        }

        void OnDisable() 
        {
            if (_entity != null)
                _entity.Destroy();
        }

        private float progress = 0;

        public void Inspect()
        {

            var changes = pegi.ChangeTrackStart();

            "Is Animating".PegiLabel().ToggleIcon(ref isAnimating).Nl();

            "Speed".PegiLabel(60).Edit(ref _speed, 0.01f, 5).Nl();

            "Intro".PegiLabel().Edit(ref _introMaterial).Nl();
            "Outro".PegiLabel().Edit(ref _outroMaterial).Nl();

        
            if (Application.isPlaying) 
            {
                "Health".PegiLabel(60).Edit(ref Health).Nl();
                if ("Replay".PegiLabel().Click().Nl())
                    Restart(Health);
            }

            if (!isAnimating)
            {
                var intro = IsIntro;

                if ("Intro".PegiLabel().ToggleIcon(ref intro).Nl())
                    IsIntro = intro;

                float anim = Animation;
                if ("Animation".PegiLabel(60).Edit(ref anim, 0, 1).Nl())
                    Animation = anim;

                float loop = IsLooping;
                if ("IsLooping".PegiLabel(60).Edit_01(ref loop).Nl())
                    IsLooping = loop;

                if ("Progress".PegiLabel(60).Edit(ref progress, 0, 3).Nl())
                {
                    IsIntro = progress < 1;
                    Animation = progress % 1;
                    IsLooping = IsIntro ? QcMath.SmoothStep(0.4f, 1f, progress) : QcMath.SmoothStep(0.4f, 1f, 3-progress);

                    if (IsIntro)
                    {
                        _emission.latestValue = QcMath.SmoothStep(1f, 0.75f, IsLooping);
                    }
                }
            }

           if (changes) 
           {
                LateUpdate();
           }
        }

        void Reset() 
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

    }

    [PEGI_Inspector_Override(typeof(C_SpriteAnimation))] internal class C_SpriteAnimationDrawer : PEGI_Inspector_Override { }
}