using QuizCanners.Inspect;
using QuizCanners.IsItGame.Pulse;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_TouchscreenMovement : IsItGameBehaviourBase, IPEGI, IGodModeCameraController
    {
        [Header("Settings")]
        [SerializeField] protected float slowDownSpeed = 15;
        [SerializeField] protected float maxSpeed = 10f;
        [SerializeField] protected float acceleration = 1f;
        [SerializeField] protected Vector3 cameraOffset = new(5, 10, 5);

        private Gate.Frame _positionUpdateFrameGate = new Gate.Frame();

        private Vector2 direction;
        private float speed01;

        protected bool ControllingCamera 
        {
            get => Singleton.TryGetValue<C_CameraOperatorGodMode, bool>(s => s.controller == (IGodModeCameraController)this, defaultValue: false, logOnServiceMissing: false);
            set
            {
                if (ControllingCamera != value)
                {
                    Singleton.Try<C_CameraOperatorGodMode>(s => s.controller = value ? this : null, logOnServiceMissing: false);
                }
            }
        }

        protected void OnEnable()
        {
            ControllingCamera = true;
        }

        protected void OnDisable()
        {
            ControllingCamera = false;
        }

        protected virtual void Start()
        {
            ControllingCamera = true;
        }

        private void CheckPosition() 
        {
            if (_positionUpdateFrameGate.TryEnter()) 
            {
                bool movementInput = false;

                if (Application.isEditor)
                {
                    Singleton.Try<C_CameraOperatorGodMode>(s =>
                    {
                        float forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
                        float right = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);

                        if (forward != 0 || right != 0)
                        {
                            movementInput = true;

                            var mainCamTf = s.MainCam.transform;
                            var fDirection = mainCamTf.forward;
                            var rDirection = mainCamTf.right;

                            fDirection.y = 0;
                            rDirection.y = 0;
                            fDirection.Normalize();
                            rDirection.Normalize();

                            direction += (fDirection * forward + rDirection * right).XZ() * Time.deltaTime;
                        }
                    }, logOnServiceMissing: false);
                }
                else
                {

                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        var inp = Input.touches[i];

                        direction += inp.deltaPosition;
                        movementInput = true;
                    }
                }

                speed01 = LerpUtils.LerpBySpeed(speed01, movementInput ? 1 : 0, movementInput ? acceleration : slowDownSpeed);

                if (!movementInput)
                {
                    direction = LerpUtils.LerpBySpeed(direction, Vector2.zero, 1);
                }

                if (direction.magnitude > 1)
                {
                    direction = direction.normalized;
                }

                var speed = speed01 * maxSpeed;

               

                var pulse = Singleton.Get<Singleton_PulseCommander>();

                bool movementHandled = false; 

                if (pulse) 
                {
                    var arena = pulse.Data.Arena;

                    if (point == null || point.GetEntity() == null) 
                    {
                        PulseArena.Point start = pulse.Data.Arena.startingPoint.GetEntity();

                        if (start != null) 
                        {
                            transform.position = LerpUtils.LerpBySpeed(transform.position, start.position, 2, out float portion);
                            if (Mathf.Approximately(portion, 1)) 
                            {
                                point = new PulseArena.Point.Id(start);
                            }

                            movementHandled = true;
                        }
                    }  else 
                    {
                        // Move by curve
                    }
                }

                if (!movementHandled)
                {
                    transform.position += speed * Time.deltaTime * new Vector3(direction.x, 0, direction.y);
                }
            }
        }

        private PulseArena.Point.Id point;
        private PulseArena.Link.Id link;

        private void Update()
        {
            CheckPosition();
        }

        #region Inspector
        public void Inspect()
        {
            pegi.Nl();
            "Speed".PegiLabel(60).Edit01(ref speed01).Nl();
            "Offset".PegiLabel().Edit(ref direction).Nl();

            Singleton.Try<C_CameraOperatorGodMode>(s => 
            {
                bool connected = ControllingCamera;

                "Controlling camera".PegiLabel().ToggleIcon(ref connected).OnChanged(()=> ControllingCamera = connected);
                s.ClickHighlight().Nl();

            } , logOnServiceMissing: false);
        }

        #endregion

        public Vector3 GetTargetPosition()
        {
            CheckPosition();
            return transform.position;
        }

        public Vector3 GetCameraOffsetPosition() => cameraOffset;
    }

    [PEGI_Inspector_Override(typeof(C_TouchscreenMovement))] internal class TouchscreenMovementDrawer : PEGI_Inspector_Override { }

}