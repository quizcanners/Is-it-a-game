using QuizCanners.Inspect;
using QuizCanners.Utils;
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

        ShaderProperty.VectorValue TIME_IN_FRAME = new ShaderProperty.VectorValue("_TimeInFrames");

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
            // propertyBlock.SetFloat("_UseCustomTime", 1.0f);
            TIME_IN_FRAME.SetOn(propertyBlock, Vector4.zero); //propertyBlock.SetFloat("_TimeInFrames", 0.0f);
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

            currentFrameTime = -currentFrameTime;

            var ceiled = Mathf.Ceil(currentFrameTime);

            float TotalLength = FramesCount + 1;

            float timeInFrames = (ceiled + 1f);// / TotalLength;

           

            TIME_IN_FRAME.SetOn(propertyBlock, new Vector4(
                x: timeInFrames,
                y: currentFrameTime,
                z: 0, 
                w: TotalLength));

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
