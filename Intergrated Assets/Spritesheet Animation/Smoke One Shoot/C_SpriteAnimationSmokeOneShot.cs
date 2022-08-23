using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_SpriteAnimationSmokeOneShot : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _speed = 1;

        ShaderProperty.FloatValue _animation = new ShaderProperty.FloatValue("_Frame");
        MaterialPropertyBlock block;

        private bool isAnimating = true;


        public void UpscaleBy(float upscale) 
        {
            transform.localScale *= upscale;
        }

        private float Animation
        {
            get => _animation.latestValue;
            set
            {
                if (block == null)
                    block = new MaterialPropertyBlock();

                _animation.SetOn(block, value);
                _meshRenderer.SetPropertyBlock(block);
            }
        }

        float _randZ;

        void Awake() 
        {
            _randZ = Random.value * 360;
        }

        public void Restart() 
        {
            Animation = 0;

        }

        void LateUpdate()
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.Z(_randZ));

            if (isAnimating)
            {
                Animation += Time.deltaTime * _speed / (1 + transform.localScale.x*0.25f);

                if (Animation >= 1)
                    Pool.Return(this);
            }
        }

        void Reset() 
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        #region Inspector

        public void Inspect()
        {
            "Is Animating".PegiLabel().ToggleIcon(ref isAnimating).Nl();

            "Speed".PegiLabel(60).Edit(ref _speed, 0.01f, 5).Nl();

            if (!isAnimating)
            {
                float anim = Animation;
                if ("Animation".PegiLabel(60).Edit(ref anim, 0, 1).Nl())
                    Animation = anim;
            }
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_SpriteAnimationSmokeOneShot))] internal class C_SpriteAnimationSmokeOneShotDrawer : PEGI_Inspector_Override { }
}