using QuizCanners.Utils;
using UnityEngine;
using QuizCanners.Inspect;

namespace QuizCanners.SpecialEffects
{
    [ExecuteInEditMode]
    public class SkyController : MonoBehaviour, IPEGI {

        public Light directional;
        public MeshRenderer skeRenderer;
        public float skyDynamics = 0.1f;
        [SerializeField] private Camera _mainCam;

        private readonly ShaderProperty.VectorValue _sunDirectionProperty = new ShaderProperty.VectorValue("_SunDirection");
        private readonly ShaderProperty.ColorFloat4Value _directionalColorProperty = new ShaderProperty.ColorFloat4Value("_Directional");
        private readonly ShaderProperty.VectorValue _offsetProperty = new ShaderProperty.VectorValue("_Off");

        private void FindComponents()
        {
            if (!skeRenderer)
                skeRenderer = GetComponent<MeshRenderer>();

            if (directional) return;

            var ls = FindObjectsOfType<Light>();

            for (var i = 0; i < ls.Length; i++)
                if (ls[i].type == LightType.Directional)
                {
                    directional = ls[i];
                    i = ls.Length;
                }
        }

        private void OnEnable()
        {
            FindComponents();
            skeRenderer.enabled = Application.isPlaying;
        }
  
        private Camera MainCam
        {
            get
            {

                if (!_mainCam)
                    _mainCam = GetComponentInParent<Camera>();

                if (!_mainCam)
                    _mainCam = Camera.main;
                
                return _mainCam;
            }
        }
        
        public virtual void Update() {

            if (directional != null) {
                var v3 = directional.transform.rotation * Vector3.back;
                _sunDirectionProperty.GlobalValue = new Vector4(v3.x, v3.y, v3.z);
                _directionalColorProperty.GlobalValue = directional.color;
            }
            
            if (!MainCam) return;

            var pos = _mainCam.transform.position * skyDynamics;

            _offsetProperty.GlobalValue = new Vector4(pos.x, pos.z, 0f, 0f);
        }

        private void LateUpdate() => transform.rotation = Quaternion.identity;

        #region Inspector
        private int _inspectedStuff = -1;

        public void Inspect()
        {
            if (_inspectedStuff == -1)
            {
                pegi.nl();

                "Main Cam".PegiLabel(90).edit(ref _mainCam).nl();
                "Directional Light".PegiLabel(90).edit(ref directional).nl();
                "Sky Renderer".PegiLabel(90).edit(ref skeRenderer).nl();
                "Sky dinamics".PegiLabel(90).edit(ref skyDynamics).nl();

                if (_mainCam)
                {
                    if (_mainCam.clearFlags == CameraClearFlags.Skybox)
                    {
                        "Skybox will hide procedural sky".PegiLabel().writeWarning();
                        if ("Set to Black Color".PegiLabel().Click())
                        {
                            _mainCam.clearFlags = CameraClearFlags.Color;
                            _mainCam.backgroundColor = Color.clear;
                        }
                    }
                }
            }

            pegi.nl();

        }
        #endregion
    }


    [PEGI_Inspector_Override(typeof(SkyController))] internal class SkyControllerDrawer : PEGI_Inspector_Override { }


}