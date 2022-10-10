using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_ChornobaivkaController : Singleton.BehaniourBase
    {
        [SerializeField] public SO_ChornobaivkaSettings _config;
        public LayerMask LayersToHit;
        public LayerMask WorldGeometryLayer;
        public int Enemies;
        public int SoftLayerToPierce;


        public PlaytimePainter_BrushConfigScriptableObject OnCollisionBrush;

        const int MAX_DISTANCE = 10000;

        public bool CastAll(Ray ray, out RaycastHit hit, float maxDistance = MAX_DISTANCE)
         => Physics.Raycast(ray, out hit, maxDistance: maxDistance, LayersToHit | (1 << SoftLayerToPierce));

        public bool CastHardSurface(Ray ray, out RaycastHit hit, float maxDistance = MAX_DISTANCE)
          => Physics.Raycast(ray, out hit, maxDistance: maxDistance, LayersToHit);

        public bool CastGeometry(Ray ray, out RaycastHit hit, float maxDistance = MAX_DISTANCE)
            => Physics.Raycast(ray, out hit, maxDistance: maxDistance, WorldGeometryLayer);


        public bool CastPierce(Ray ray, out RaycastHit softHit, float maxDistance = MAX_DISTANCE)
            => Physics.Raycast(ray, out softHit, maxDistance: maxDistance, (1 << SoftLayerToPierce));

        public bool CastPierce(Ray ray, RaycastHit hardHit, out RaycastHit softHit)
        {
            float dist = Vector3.Distance(hardHit.point, ray.origin) - 0.05f;
            if (dist <= 0)
            {
                softHit = default(RaycastHit);
                return false;
            }

            return Physics.Raycast(ray, out softHit, maxDistance: dist, (1 << SoftLayerToPierce));
        }


        int TargetFramerate => Application.targetFrameRate > 1 ? Application.targetFrameRate : 60;


        private void Update()
        {
            Time.timeScale = LerpUtils.LerpBySpeed(Time.timeScale, Input.GetKey(KeyCode.T) ? 0.2f: 1f, 0.5f, unscaledTime: true);

            Time.fixedDeltaTime =  Mathf.Clamp(Time.timeScale / TargetFramerate, min: 0.001f ,max: 0.02f);

        }


        protected override void AfterEnable()
        {
            base.AfterEnable();

            _config.UpdateShaderParameters();
        }

        public override void Inspect()
        {
            base.Inspect();
            "Layers To Hit".PegiLabel(80).Edit_Property(() => LayersToHit, this).Nl();

            "Enemies".PegiLabel(80).Edit_Layer(ref Enemies).Nl();
            "Layer To Pierce".PegiLabel(80).Edit_Layer(ref SoftLayerToPierce).Nl();

            "Config".PegiLabel().Edit_Inspect(ref _config).Nl();

            /*
            var dt = Time.fixedDeltaTime;
            if ("Fixed Delta Time".PegiLabel().Edit(ref dt).Nl())
                Time.fixedDeltaTime = dt;*/

            if (Application.isPlaying) 
            {
                "Target FrameRate: {0}".F(TargetFramerate).PegiLabel().Nl();
                "Hold T to Slow Time: {0}%".F(Mathf.FloorToInt(Time.timeScale * 100)).PegiLabel().WriteHint().Nl();
                "Fixed Delta Time: {0} ({1} Update/Second)".F(Time.fixedDeltaTime, Mathf.RoundToInt( 1f/Time.fixedDeltaTime) ).PegiLabel().Nl();
            }

        }




    }

    [PEGI_Inspector_Override(typeof(Singleton_ChornobaivkaController))] internal class Singleton_ChornobaivkaControllerDrawer : PEGI_Inspector_Override {}
}