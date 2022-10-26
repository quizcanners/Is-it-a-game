using QuizCanners.Inspect;
using QuizCanners.IsItGame.SplinePath;
using QuizCanners.Utils;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_Player : Singleton.BehaniourBase, IGodModeCameraController
    {
        [Header("Settings")]
        [SerializeField] private MovementMode _mode;
        [SerializeField] protected Vector3 cameraOffset = new(5, 10, 5);

        private Vector2 direction;
        [SerializeField] private bool _targetControllingCameraState;
        [NonSerialized] private SO_SplinePath.Unit _unit;


        private enum MovementMode { None, God, Spline, Navmesh }

        private readonly Gate.Frame _frame = new();

        protected bool ControllingCamera 
        {
            get => Singleton.TryGetValue<Singleton_CameraOperatorConfigurable, bool>(s => s.controller == (IGodModeCameraController)this, defaultValue: false, logOnServiceMissing: false);
            set
            {
                if (ControllingCamera != value)
                {
                    Singleton.Try<Singleton_CameraOperatorConfigurable>(s => s.controller = value ? this : null, logOnServiceMissing: false);
                }
            }
        }

        protected override void OnAfterEnable()
        {
            base.OnAfterEnable();
            ControllingCamera = _targetControllingCameraState;
        }


        protected override void OnBeforeOnDisableOrEnterPlayMode(bool afterEnableCalled)
        {
            base.OnBeforeOnDisableOrEnterPlayMode(afterEnableCalled);
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

        bool TryGetRawDirection(out float forward, out float right) 
        {
            forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
            right = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);

            return forward != 0 || right != 0;
        }

        private void Update()
        {
            CheckPosition();
        }

        void CheckPosition() 
        {
            if (_frame.TryEnter())
            {
                switch (_mode) 
                {
                    case MovementMode.Navmesh:

                        if (TryGetRawDirection(out float forward1, out float right1))
                        {
                            // Handled by Player Movement


                        }


                        break;
                    case MovementMode.Spline:
                        Singleton.Try<Singleton_SplinePath>(s =>
                        {
                            _unit ??= s.CreateUnit(isPlayer: true);

                            if (_unit != null)
                            {
                                if (TryGetRawDirection(out float forward, out float right))
                                {
                                    var direction = RotatedDirection(forward: forward, right: right);
                                    _unit.TryMove(10 * Time.deltaTime * direction);
                                }
                                transform.position = _unit.GetPosition();
                                // handled = true;
                            }
                        });
                        break;
                    case MovementMode.God:
                        if (TryGetRawDirection(out float forward, out float right))
                        {
                            var direction = RotatedDirection(forward: forward, right: right);
                            transform.position += direction * (Input.GetKey(KeyCode.LeftShift) ? 10 : 5) * Time.deltaTime;
                        }
                        break;
                }
            }
        }


        #region Inspector
        public override void Inspect()
        {
            pegi.Nl();

            if (Application.isPlaying == false)
            {
                pegi.TryDefaultInspect(this);
                return;
            }

            "Mode".PegiLabel(60).Edit_Enum(ref _mode).Nl();

            if (ControllingCamera)
            {
                "Offset".PegiLabel().Edit(ref direction).Nl();
            }

            Singleton.Try<Singleton_CameraOperatorGodMode>(s =>
            {
                bool connected = ControllingCamera;

                "Controlling camera".PegiLabel().ToggleIcon(ref connected).OnChanged(() => ControllingCamera = connected);
                pegi.ClickHighlight(s).Nl();

            }, logOnServiceMissing: false);

            

            switch (_mode) 
            {

                case MovementMode.Spline:
                    "Path Curve".PegiLabel(pegi.Styles.ListLabel).Nl();
                    _unit.Nested_Inspect();
                    break;
            }

        }

        public override string NeedAttention()
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

        public bool TryGetCameraHeight(out float height)
        {
            height = 1;
            return false;
        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_Player))] internal class TouchscreenMovementDrawer : PEGI_Inspector_Override { }

}