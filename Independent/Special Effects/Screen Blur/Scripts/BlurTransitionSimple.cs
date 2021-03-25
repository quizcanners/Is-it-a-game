using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static QuizCanners.Utils.QcUtils;

namespace QuizCanners.SpecialEffects
{
    [DisallowMultipleComponent]
    public class BlurTransitionSimple : MonoBehaviour, IPEGI
    {
        [SerializeField] private Image _blurImage;
        [SerializeField] private ScreenBlurController.ProcessCommand mode = ScreenBlurController.ProcessCommand.Blur;
        [SerializeField] private float _transitionSpeed = 6f;

        ShaderProperty.MaterialToggle FADE_TO_CENTER = new ShaderProperty.MaterialToggle("FADE_TO_CENTER");

        [Header("Optional:")]
        [NonSerialized] private MaterialInstancer _materialInstancer;
        private Material Material
        {
            get
            {
                if (_materialInstancer == null)
                    _materialInstancer = new MaterialInstancer(_blurImage);

                return _materialInstancer.MaterialInstance;
            }
        }

        public void SetObscure(Action onObscured) => SetObscure(onObscured, mode);

        public void SetObscure(Action onObscured, ScreenBlurController.ProcessCommand transitionMode)
        {
            Singleton.Try<ScreenBlurController>(x => x.RequestUpdate(onFirstRendered: () =>
            {
                ObscureInternal();
                onObscured?.Invoke();
            }, afterScreenGrab: transitionMode));
        }

        public IEnumerator SetObscureIEnumerator() => SetObscureIEnumerator(mode);

        public IEnumerator SetObscureIEnumerator(ScreenBlurController.ProcessCommand transitionMode)
        {
            bool done = false;
            SetObscure(onObscured: () => done = true, transitionMode);
            while (!done)
                yield return null;
        }

        public IEnumerator TransitionIEnumerator() => TransitionIEnumerator(mode);

        public IEnumerator TransitionIEnumerator(ScreenBlurController.ProcessCommand transitionMode)
        {
            bool done = false;
            Transition(onObscured: () => done = true, transitionMode);
            while (!done)
                yield return null;
        }

        public void Transition(Action onObscured) => Transition(onObscured, mode);

        public void Transition(Action onObscured, ScreenBlurController.ProcessCommand transitionMode)
        {
            Singleton.Try<ScreenBlurController>(s => s.RequestUpdate(onFirstRendered: () =>
            {
                ObscureInternal();
                onObscured?.Invoke();
                Reveal();

                var mat = Material;
                if (mat) 
                {
                    FADE_TO_CENTER.SetOn(mat, transitionMode == ScreenBlurController.ProcessCommand.ZoomOut);
                }

            }, afterScreenGrab: transitionMode));
        }

        public void ObscureInternal() 
        {
            _blurImage.TrySetAlpha(1);
            _blurImage.raycastTarget = true;
        }

        public void Reveal(bool skipAnimation = false)
        {
            _blurImage.raycastTarget = false;

            if (skipAnimation)
            {
                _blurImage.TrySetAlpha(0);
                enabled = false;
            }
            else
            {
                enabled = true;
            }
        }

        protected virtual void Awake()
        {
            Reveal(skipAnimation: true);
        }

        void Update()
        {
            if (_blurImage.color.a > 0)
            {
                _blurImage.IsLerpingAlphaBySpeed(0, _transitionSpeed);
            } else 
            {
                enabled = false;
            }
        }

        public void Inspect()
        {
            pegi.nl();
            pegi.edit_ifNull(ref _blurImage, gameObject).nl();

            "Transition speed".PegiLabel(120).edit(ref _transitionSpeed).nl();

            "Transition mode".PegiLabel(140).editEnum(ref mode).nl();

            if ("Transition Test".PegiLabel().Click())
                Transition(null);
        }

        void Reset()
        {
            if (!_blurImage)
                _blurImage = GetComponent<Image>();
        }

    }
    
    [PEGI_Inspector_Override(typeof(BlurTransitionSimple))]
    internal class BlurTransitionSimpleDrawer : PEGI_Inspector_Override { }
}
