using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class BFX_BloodController : MonoBehaviour, INeedAttention
    {
        [SerializeField] private BFX_ShaderProperies _decal;
        [SerializeField] private BFX_ManualAnimationUpdate _blood;

        public float AnimationSpeed = 1;
        public float GroundHeight = 0;
        [Range(0, 1)]
        public float LightIntensityMultiplier = 1;
        public bool FreezeDecalDisappearance = false;
        public _DecalRenderinMode DecalRenderinMode = _DecalRenderinMode.Floor_XZ;
        public bool ClampDecalSideSurface = false;

        public enum _DecalRenderinMode
        {
            Floor_XZ,
            AverageRayBetwenForwardAndFloor
        }

        private void LateUpdate()
        {
            if (_decal && _decal.IsVisible)
                return;

            if (_blood && _blood.Progress<1)
                return;

            Pool.Return(this);
        }

        private void Reset()
        {
            _decal = GetComponentInChildren<BFX_ShaderProperies>();
            _blood = GetComponentInChildren<BFX_ManualAnimationUpdate>();
        }

        private void Start()
        {
            if (!_decal || !_blood)
                Reset();
        }

        public string NeedAttention()
        {
            return null;
        }
    }
}