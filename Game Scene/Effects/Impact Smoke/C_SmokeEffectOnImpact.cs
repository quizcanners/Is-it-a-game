using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class C_SmokeEffectOnImpact : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private readonly LinkedLerp.ShaderFloat _visibility = new("_Visibility", 1);
        private readonly LinkedLerp.ShaderColor _color = new("_Color", Color.white, maxSpeed: 10);

        private MaterialPropertyBlock block;
        private LogicWrappers.Request _propertyBlockDirty = new LogicWrappers.Request();
        private bool _animating = false;
        private float smokeDensity;

        float _visibilityValue;

        public float Visibility 
        {
            get => _visibilityValue;  
            set 
            {
                _visibilityValue = value;
                CheckBlock();
                _visibility.CurrentValue = Mathf.SmoothStep(0,1, value);
                _visibility.Property.SetOn(block);
                _propertyBlockDirty.CreateRequest();
            }
        }

        public Color Color
        {
            get => _color.CurrentValue;
            set
            {
                CheckBlock();
                _color.CurrentValue = value;
                _color.Property.SetOn(block);
                _propertyBlockDirty.CreateRequest();
            }
        }

        public float Size 
        {
            get => transform.localScale.x;
            set => transform.localScale = Vector3.one * value;
        }

        public void TryConsume(C_SmokeEffectOnImpact other) 
        {
            var dist = (other.transform.position - transform.position).magnitude;
            float sizes = Size + other.Size;

            if (sizes*0.5 > dist) 
            {
                if (Size > other.Size)
                {
                    if (Visibility > 0.5f)
                       ConsumeToSelf(other);
                }
                else if (other.Visibility>0.5f)
                    other.ConsumeToSelf(this);
            }
        }

        private void ConsumeToSelf(C_SmokeEffectOnImpact other) 
        {
            smokeDensity += other.smokeDensity * 0.8f;
            other.smokeDensity *= 0.2f;
        }

        public void PlayAnimateFromDot(float density = 0.5f) 
        {
            Size = 0.3f;
            Visibility = 1;
            smokeDensity += density;
            _color.CurrentValue = new Color(0.5f, 0.4f, 0.3f);
            _animating = true;
        }

        internal void Refresh()
        {
            smokeDensity = 0;
            PlayAnimateFromDot();
            CheckBlock();
            meshRenderer.SetPropertyBlock(block);
        }

        public void PlayFromBigCloud(float size)
        {
            Size = size;
            smokeDensity += size;
            Visibility = 0;
            _color.CurrentValue = Color.white;
            _animating = true;
        }
    
        private void CheckBlock() 
        {
            if (block == null)
                block = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            if (meshRenderer && _propertyBlockDirty.TryUseRequest()) 
            {
                CheckBlock();
                meshRenderer.SetPropertyBlock(block);
            }

            if (_animating)
            {
                Size += Time.deltaTime * (1 + Mathf.Pow(smokeDensity, 1.4f));
                float deSize = 1f / Size;
                float targetVisibility = Mathf.Clamp(smokeDensity / Size , 0, max: deSize);
                bool isFading = targetVisibility < Visibility;
                Visibility = LerpUtils.LerpBySpeed(Visibility, Mathf.Clamp01(targetVisibility), (1 + Mathf.Abs(targetVisibility - Visibility)) 
                    / (isFading ? (1 + smokeDensity) : Size));
                Color = LerpUtils.LerpBySpeed(Color, Color.white, 1);
                float fadeSpeed = Size * Size * Visibility * 0.1f * (1 + Pool_SmokeEffects.InstancesCount);
                smokeDensity = Mathf.Max(0, smokeDensity - fadeSpeed * Time.deltaTime);

                if (smokeDensity < 0.01f && Visibility < 0.01f) 
                {
                    Pool_SmokeEffects.ReturnToPool(this);
                }
            }
        }

        #region Inspector

        public void Inspect()
        {
            var changed = pegi.ChangeTrackStart();

            pegi.Nl();

            "Mesh Rederer".PegiLabel(90).Edit_IfNull(ref meshRenderer, gameObject).Nl();

            var vis = Visibility;
            "Visibility".PegiLabel(width: 60).Edit_01(ref vis).Nl().OnChanged(()=> Visibility = vis);

            var size = Size;
            "Size".PegiLabel(width: 50).Edit(ref size, 0.01f, 5f).Nl().OnChanged(()=> Size = size);

            var col = Color;
            "Color".PegiLabel(width: 50).Edit(ref col).Nl().OnChanged(() => Color = col);

            if ((_animating ? Icon.Pause : Icon.Play).Click())
                _animating = !_animating;
            "Density".PegiLabel(60).Edit(ref smokeDensity).Nl();

            pegi.Click(()=> PlayAnimateFromDot(0.5f));

            "Big Cloud".PegiLabel().Click().Nl().OnChanged(()=> PlayFromBigCloud(10));

            if (changed)
                _propertyBlockDirty.CreateRequest();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_SmokeEffectOnImpact))] internal class C_SmokeEffectOnImpactDrawer : PEGI_Inspector_Override { }
}
