using UnityEngine;
using System;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class BFX_ShaderProperies : MonoBehaviour
    {
        public BFX_BloodController BloodSettings;
        [SerializeField] private Renderer rend;

        public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;

        [NonSerialized] public bool IsVisible;

        ShaderProperty.FloatValue CUTOUT = new ShaderProperty.FloatValue("_Cutout");
        ShaderProperty.VectorValue FORWARD = new ShaderProperty.VectorValue("_DecalForwardDir");

        //private int cutoutPropertyID;
        //int forwardDirPropertyID;
        float timeLapsed;

        private MaterialPropertyBlock props;
        public event Action OnAnimationFinished;



        private void OnEnable()
        {
            if (!rend)
                rend = GetComponent<Renderer>();

            if (props == null)
                props = new MaterialPropertyBlock();

           // cutoutPropertyID = Shader.PropertyToID("_Cutout");
            //forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");


            // startTime = Time.time + TimeDelay;
            IsVisible = true;

            //rend.GetPropertyBlock(props);

            //var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
            //props.SetFloat(cutoutPropertyID, eval);

            timeLapsed = 0;

            UpdateValues();
            FORWARD.SetOn(props, transform.up);
           // props.SetVector(forwardDirPropertyID, transform.up);
            rend.SetPropertyBlock(props);
        }

        private void OnDisable()
        {
            if (rend && props!= null)
            {
                //rend.GetPropertyBlock(props);
                timeLapsed = 0;
                UpdateValues();
                rend.SetPropertyBlock(props);
            }

        }

        private void UpdateValues()
        {
            var eval = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
            CUTOUT.SetOn(props, eval);
           //props.SetFloat(cutoutPropertyID, eval);
        }

        private void Update()
        {
            if (!IsVisible || !rend || props == null)
                return;

            //rend.GetPropertyBlock(props);

            var needFreeze = BloodSettings && BloodSettings.FreezeDecalDisappearance && (timeLapsed / GraphTimeMultiplier) <= 0.3f;

            if (!needFreeze)
            {
                float animationSpeed = 1 / (0.5f + Pool.VacancyFraction<BFX_BloodController>());
                var deltaTime = BloodSettings ? Time.deltaTime * BloodSettings.AnimationSpeed : Time.deltaTime;
                timeLapsed += deltaTime * animationSpeed;
            }

            //  var eval = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
            // props.SetFloat(cutoutPropertyID, eval);
            UpdateValues();

         //   if (BloodSettings != null) props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));

            if (timeLapsed >= GraphTimeMultiplier)
            {
                IsVisible = false;
                OnAnimationFinished?.Invoke();
            }

            FORWARD.SetOn(props, transform.up);
            //props.SetVector(forwardDirPropertyID, transform.up);
            rend.SetPropertyBlock(props);
        }

    }
}
