using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame 
{
    public class BFX_ManualAnimationUpdate : MonoBehaviour, IPEGI
    {
        public BFX_BloodController BloodSettings;
        public AnimationCurve AnimationSpeed = AnimationCurve.Linear(0, 0, 1, 1);
        public float FramesCount = 99;
        public float TimeLimit = 3;
        public float OffsetFrames = 0;

        private bool _animating = true;


        public float Progress 
        {
            get => currentTime / TimeLimit;
            set => currentTime = TimeLimit * value;
        }

        private float currentTime;

        Renderer rend;
        private MaterialPropertyBlock propertyBlock;

        void OnEnable()
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            if (!rend)
                rend = GetComponent<Renderer>();

            rend.enabled = true;

            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_UseCustomTime", 1.0f);
            propertyBlock.SetFloat("_TimeInFrames", 0.0f);
            rend.SetPropertyBlock(propertyBlock);

            currentTime = 0;
            _animating = true;
        }

        void Update()
        {
            if (_animating && BloodSettings)
            {
                currentTime += Time.deltaTime * BloodSettings.AnimationSpeed;

                if (Progress >= 1)
                {
                    if (rend.enabled)
                        rend.enabled = false;

                    return;
                }
            }

            var currentFrameTime = AnimationSpeed.Evaluate(currentTime / TimeLimit);
            currentFrameTime = currentFrameTime * FramesCount + OffsetFrames + 1.1f;
            float timeInFrames = 
                Mathf.Ceil(-currentFrameTime) / (FramesCount + 1) 
                + (1.0f / (FramesCount + 1));

            rend.GetPropertyBlock(propertyBlock);
            //propertyBlock.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
            propertyBlock.SetFloat("_TimeInFrames", timeInFrames);
            rend.SetPropertyBlock(propertyBlock);
        }

        public void Inspect()
        {
            "Animating".PegiLabel().ToggleIcon(ref _animating).Nl();

            var progress = Progress;
            "Animation".PegiLabel().Edit_01(ref progress).OnChanged(() => Progress = progress).Nl();
        }
    }

    [PEGI_Inspector_Override(typeof(BFX_ManualAnimationUpdate))] internal class BFX_ManualAnimationUpdateDrawer : PEGI_Inspector_Override { }
}
