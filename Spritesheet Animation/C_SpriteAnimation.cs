using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_SpriteAnimation : MonoBehaviour, IPEGI
    {
        //  [SerializeField] private ParticleSystem particles;

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _introMaterial;
        [SerializeField] private Material _outroMaterial;
        [SerializeField] private float _speed = 1;

        ShaderProperty.FloatValue _animation = new ShaderProperty.FloatValue("_Frame");
        ShaderProperty.FloatValue _loopVisibility = new ShaderProperty.FloatValue("_LoopVisibility");
        MaterialPropertyBlock block;

        LogicWrappers.Request blockDirty = new LogicWrappers.Request();

        private bool isAnimating;
        private bool _isIntro;

        public float Health;

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

        void LateUpdate() 
        {

            if (isAnimating)
            {
               

           
                Animation += Time.deltaTime * _speed;

               

                if (IsIntro)
                {
                    IsLooping = Mathf.Max(IsLooping, QcMath.SmoothStep(0.4f, 1f, Animation)); 

                    Health -= Time.deltaTime;

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

                    IsLooping = Mathf.Min(IsLooping, QcMath.SmoothStep(0.4f, 0f, Animation));

                    if (Animation >= 1) 
                    {
                        Animation = 1;

                        // Destroy
                    }
                }
            }

            if (blockDirty.TryUseRequest())
            {
                if (block == null)
                    block = new MaterialPropertyBlock();

                _animation.SetOn(block);
                _loopVisibility.SetOn(block);

                _meshRenderer.SetPropertyBlock(block);
            }
        }

        void Restart(float health) 
        {
            Health = health;
            IsLooping = 0;
            Animation = 0;
            IsIntro = true;
            isAnimating = true;
        }

        void OnEnable() 
        {
            Restart(Health);
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
                    Animation = progress % 1;
                    IsLooping = IsIntro ? QcMath.SmoothStep(0.4f, 1f, Animation) : QcMath.SmoothStep(0.4f, 0f, Animation);
                }
            }

           if (changes) 
           {
                LateUpdate();
           }

            /*  "Particle System".PegiLabel().Edit_IfNull(ref particles, gameObject).Nl();

              if ("Emit".PegiLabel().Click())
                  particles.Emit(1);

              if ("Start".PegiLabel().Click())
                  particles.Play(true);

              if ("Stop".PegiLabel().Click())
                  particles.Stop(withChildren: true, stopBehavior: ParticleSystemStopBehavior.StopEmitting);*/

            //  if ("Animate".PegiLabel().Click())
            //   particles.


        }

        void Reset() 
        {
            _meshRenderer = GetComponent<MeshRenderer>();
           //  particles = GetComponent<ParticleSystem>();

            // if (!particles)
            //  particles = GetComponentInChildren<ParticleSystem>();
        }

    }

    [PEGI_Inspector_Override(typeof(C_SpriteAnimation))] internal class C_SpriteAnimationDrawer : PEGI_Inspector_Override { }
}