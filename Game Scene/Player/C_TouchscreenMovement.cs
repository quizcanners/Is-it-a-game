using QuizCanners.Inspect;
using QuizCanners.IsItGame.Pulse;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_TouchscreenMovement : IsItGameBehaviourBase, IPEGI, IGodModeCameraController, INeedAttention
    {
        [Header("Settings")]
        [SerializeField] protected Vector3 cameraOffset = new(5, 10, 5);

        private Vector2 direction;
        [SerializeField] private bool _targetControllingCameraState;
        [NonSerialized] private PulsePath.Unit _unit;


        private Gate.Frame frame = new Gate.Frame();

        protected bool ControllingCamera 
        {
            get => Singleton.TryGetValue<Singleton_CameraOperatorGodMode, bool>(s => s.controller == (IGodModeCameraController)this, defaultValue: false, logOnServiceMissing: false);
            set
            {
                if (ControllingCamera != value)
                {
                    Singleton.Try<Singleton_CameraOperatorGodMode>(s => s.controller = value ? this : null, logOnServiceMissing: false);
                }
            }
        }

        protected void OnEnable()
        {
            ControllingCamera = _targetControllingCameraState;
        }

        protected void OnDisable()
        {
            _targetControllingCameraState = ControllingCamera;
            ControllingCamera = false;
        }

        protected virtual void Start()
        {

        }

        Vector3 RotatedDirection(float forward, float right) 
        {
            if (!Camera.main)
                return Vector3.zero;

            var mainCamTf = Camera.main.transform;
            var fDirection = mainCamTf.forward;
            var rDirection = mainCamTf.right;

            fDirection.y = 0;
            rDirection.y = 0;
            fDirection.Normalize();
            rDirection.Normalize();

           return (fDirection * forward + rDirection * right);
        }

        void GetRawDirection(out float forward, out float right) 
        {
            forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
            right = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);
        }

        private void Update()
        {
            CheckPosition();
        }

        void CheckPosition() 
        {
            if (frame.TryEnter()) 
            {
                GetRawDirection(out float forward, out float right);

                bool handled = false;

                Singleton.Try<Singleton_PulsePath>(s =>
                {
                    if (_unit == null)
                    {
                        _unit = Singleton.TryGetValue<Singleton_PulsePath, PulsePath.Unit>(
                            s => s.CreateUnit(isPlayer: true));
                    }

                    if (_unit != null)
                    {
                        if (forward != 0 || right != 0)
                        {
                            var direction = RotatedDirection(forward: forward, right: right);
                            _unit.TryMove(direction * 10 * Time.deltaTime);
                        }
                        transform.position = _unit.GetPosition();
                        handled = true;
                    }
                });

               //if (!handled)
                   // transform.position += direction * (Input.GetKey(KeyCode.LeftShift) ? 10 : 5) * Time.deltaTime;
                
            }
        }
             

        #region Inspector
        public void Inspect()
        {
            pegi.Nl();

            if (Application.isPlaying == false)
            {
                pegi.TryDefaultInspect(this);
                return;
            }

            "Offset".PegiLabel().Edit(ref direction).Nl();

            Singleton.Try<Singleton_CameraOperatorGodMode>(s => 
            {
                bool connected = ControllingCamera;

                "Controlling camera".PegiLabel().ToggleIcon(ref connected).OnChanged(()=> ControllingCamera = connected);
                s.ClickHighlight().Nl();

            } , logOnServiceMissing: false);

            "Path Curve".PegiLabel(pegi.Styles.ListLabel).Nl();
            _unit.Nested_Inspect();

        }

        public string NeedAttention()
        {
            if (!Camera.main)
                return "No Main Camera Found";

            return null;
        }

        #endregion


        Vector3 IGodModeCameraController.GetTargetPosition()
        {
            CheckPosition();
            return transform.position;
        }
        Vector3 IGodModeCameraController.GetCameraOffsetPosition() => cameraOffset;
    }

    [PEGI_Inspector_Override(typeof(C_TouchscreenMovement))] internal class TouchscreenMovementDrawer : PEGI_Inspector_Override { }

}