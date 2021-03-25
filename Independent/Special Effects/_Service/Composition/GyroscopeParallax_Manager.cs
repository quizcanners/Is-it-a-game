using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{

    public partial class SpecialEffectShadersService
    {
        [Serializable]
        public class GyroscopeParallaxManager : IPEGI, IPEGI_ListInspect
        {
            [SerializeField] private bool _enabled = true;
            [SerializeField] private float _sensitivity = 0.1f;
            [SerializeField] private float _centralizeSpeed = 1;
            [SerializeField] private bool _emulateTiltsInEditor = true;

            [NonSerialized] private bool initialized;
            [NonSerialized] private Quaternion previousRotation;
            [SerializeField] private ShaderProperty.Feature USE_PARALLAX = new ShaderProperty.Feature("qc_USE_PARALLAX");
            [NonSerialized] private Vector2 _debugTiltSpeed;

            private readonly ShaderProperty.VectorValue PARALLAX_OFFSET = new ShaderProperty.VectorValue("qc_ParallaxOffset");

            public bool Enabled 
            {
                get => _enabled;
                set 
                {
                    _enabled = value;
                    Input.gyro.enabled = _enabled;
                    USE_PARALLAX.Enabled = _enabled; // To Set the value
                }
            }

            public Vector2 AccumulatedOffset { get; private set; }

            private Gyroscope Gyro => Input.gyro;

            public void ManagedOnEnable()
            {
                Enabled = _enabled;
            }

            public void ManagedLateUpdate()
            {
                if (Application.isPlaying == false || !Enabled)
                {
                    return;
                }

                float deltaTime = Mathf.Min(Time.deltaTime, 0.01f);

                if (!Application.isEditor)
                {
                    var rate = Input.gyro.rotationRateUnbiased;
                    AccumulatedOffset += _sensitivity * deltaTime * new Vector2(rate.y, -rate.x);

                }
                else
                {
                    if (_emulateTiltsInEditor)
                    {
                        _debugTiltSpeed = (Input.mousePosition.XY() - new Vector2(Screen.width, Screen.height) * 0.5f) * 0.1f / ((float)Screen.height);
                        AccumulatedOffset += new Vector2(_debugTiltSpeed.x, _debugTiltSpeed.y) * deltaTime;
                    }
                    else
                    {
                        if (!initialized)
                        {
                            initialized = true;
                            previousRotation = Gyro.attitude;
                            return;
                        }

                        Quaternion delta = previousRotation * Quaternion.Inverse(Gyro.attitude);

                        Vector2 screenSizeCorrection = new Vector2(Screen.width, Screen.height).normalized * 2;

                        AccumulatedOffset += _sensitivity * 360 * new Vector2(delta.y, -delta.x) * screenSizeCorrection;
                        previousRotation = Gyro.attitude;
                    }
                }

                var fadeCoef = Mathf.Pow(AccumulatedOffset.magnitude * 50f, 2) / 50f;

                AccumulatedOffset = Vector2.Lerp(AccumulatedOffset, Vector2.zero, deltaTime * fadeCoef * _centralizeSpeed);
                PARALLAX_OFFSET.GlobalValue = AccumulatedOffset.ToVector4();
            }

            #region Inspector
            public void Inspect()
            {
                pegi.nl();

                var on = Gyro.enabled;
                if (" Gyro.enabled".PegiLabel().toggleIcon(ref on).nl())
                    Gyro.enabled = on;

                if (Application.isEditor)
                {
                    "Previous: {0}".F(previousRotation).PegiLabel().nl();
                    "Emulate in Editor".PegiLabel().toggleIcon(ref _emulateTiltsInEditor).nl();
                }

                "Offset: {0}".F(AccumulatedOffset).PegiLabel().nl();

                "Sensitivity".PegiLabel().edit(ref _sensitivity).nl();

                "Fade Speed".PegiLabel().edit(ref _centralizeSpeed).nl();

                USE_PARALLAX.Nested_Inspect().nl();

                if (USE_PARALLAX.Enabled)
                    PARALLAX_OFFSET.ToString().PegiLabel().write_ForCopy(showCopyButton: true).nl();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (pegi.toggleIcon(ref _enabled))
                    Enabled = _enabled;

                if ("Gyroscope".PegiLabel().ClickLabel() | icon.Enter.Click())
                    edited = index;
            }

            #endregion
        }
    }
}