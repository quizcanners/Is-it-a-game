using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class C_SmokeEffectOnImpact : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private LinkedLerp.ShaderFloat _visibility = new LinkedLerp.ShaderFloat("_Visibility", 1);
        private LinkedLerp.ShaderColor _color = new LinkedLerp.ShaderColor("_Color", Color.white, maxSpeed: 3);

        private MaterialPropertyBlock block;
        private LogicWrappers.Request _propertyBlockDirty = new LogicWrappers.Request();
        private bool _animating = false;
        private float smokeDensity;

        public float Visibility 
        {
            get => _visibility.CurrentValue;
            set 
            {
                CheckBlock();
                _visibility.CurrentValue = value;
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
            set 
            {
                transform.localScale = Vector3.one * value;
            }
        }

        public void TryConsume(C_SmokeEffectOnImpact other) 
        {
            var dist = (other.transform.position - transform.position).magnitude;

            float dens = Size + other.Size;

            if ((dens + 0.5f) > dist) 
            {
                if (Size > other.Size)
                    ConsumeToSelf(other);
                else
                    other.ConsumeToSelf(this);
            }
        }

        private void ConsumeToSelf(C_SmokeEffectOnImpact other) 
        {
            smokeDensity += other.smokeDensity;
            other.smokeDensity = 0;
        }

        public void PlayAnimateFromDot() 
        {
            Size = 0.1f;
            Visibility = 1;
            smokeDensity = 1;
            _color.CurrentValue = new Color(0.3f, 0.2f, 0.1f);
            _animating = true;
        }

        public void PlayFromBigCloud(float size)
        {
            Size = size;
            smokeDensity = size;
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
            if (meshRenderer!= null && _propertyBlockDirty.TryUseRequest()) 
            {
                CheckBlock();
                meshRenderer.SetPropertyBlock(block);
            }

            if (_animating)
            {
                float deSize = 1f / Size;
                Size += Time.deltaTime * deSize;

                float targetVisibility = Mathf.Clamp(smokeDensity / Size , 0, deSize);

                bool isFading = targetVisibility < Visibility;

                Visibility = LerpUtils.LerpBySpeed(Visibility, Mathf.Clamp01(targetVisibility), (1 + Mathf.Abs(targetVisibility - Visibility)) 
                    / (isFading ? (1 + smokeDensity) : Size));

                Color = LerpUtils.LerpBySpeed(Color, Color.white, 1);

                smokeDensity = Mathf.Max(0, smokeDensity - Size * Size * Size * Mathf.PI * Visibility * Time.deltaTime);

                if (smokeDensity < 0.01f && Visibility < 0.01f) 
                {
                    Singleton_SmokeEffectController.OnFinished(this);
                }
            }
        }

       

        #region Inspector

        public void Inspect()
        {
            var changed = pegi.ChangeTrackStart();

            pegi.Nl();

            "Mesh Rederer".PegiLabel(90).Edit_IfNull(ref meshRenderer, gameObject).Nl();

            if (block == null)
                block = new MaterialPropertyBlock();

            var vis = Visibility;
            "Visibility".PegiLabel(width: 60).Edit01(ref vis).Nl().OnChanged(()=> Visibility = vis);

            var size = Size;
            "Size".PegiLabel(width: 50).Edit(ref size, 0.01f, 5f).Nl().OnChanged(()=> Size = size);

            if ((_animating ? Icon.Pause : Icon.Play).Click())
                _animating = !_animating;
            "Density".PegiLabel(60).Edit(ref smokeDensity).Nl();

            pegi.Click(PlayAnimateFromDot);

            "Big Cloud".PegiLabel().Click().Nl().OnChanged(()=> PlayFromBigCloud(10));

            if (changed)
                _propertyBlockDirty.CreateRequest();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_SmokeEffectOnImpact))] internal class C_SmokeEffectOnImpactDrawer : PEGI_Inspector_Override { }
}
