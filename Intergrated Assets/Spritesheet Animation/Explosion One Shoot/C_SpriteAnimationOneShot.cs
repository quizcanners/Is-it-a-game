using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_SpriteAnimationOneShot : MonoBehaviour, IPEGI
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

                _animation.SetOn(block, value % 1);// 1-Mathf.Pow(1-value,1.2f));
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
                float newState = Animation + Time.deltaTime * _speed / (1 + transform.localScale.x * 0.25f);

                if (newState >= 1)
                    Pool.Return(this);
                else
                    Animation = newState;
            }
        }

        void Reset() 
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        #region Inspector

        public void Inspect()
        {
            "Is Animating".PegiLabel().ToggleIcon(ref isAnimating);

            if (isAnimating && Icon.Refresh.Click())
                Animation = 0;

            pegi.Nl();

            

            if (!isAnimating)
            {
                float anim = Animation;
                if ("Animation".PegiLabel(60).Edit(ref anim, 0, 1).Nl())
                    Animation = anim;
            } else 
            {
                "Speed".PegiLabel(60).Edit(ref _speed, 0.01f, 5).Nl(()=>
                {
                    if (Animation>0.5f)
                        Animation = 0;
                });
            }
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_SpriteAnimationOneShot))] internal class C_SpriteAnimationOneShotDrawer : PEGI_Inspector_Override { }
}