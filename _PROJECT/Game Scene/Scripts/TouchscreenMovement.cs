using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class TouchscreenMovement : IsItGameBehaviourBase, IPEGI
    {
        [SerializeField] private CharacterController _character;

        [Header("Settings")]
        [SerializeField] private float _slowDownSpeed = 15;
        [SerializeField] private float _maxSpeed = 10f;
        [SerializeField] private float _acceleration = 1f;
        [SerializeField] private float _gravity = 9.8f;
      //  [SerializeField] private float _jumpHeight = 2;

        private Vector2 direction;
        private float speed01;


      //  float JumpVelocity => Mathf.Sqrt(_jumpHeight * (-2 * _gravity));

        private void Update()
        {
            bool movementInput = false;

            if (Application.isEditor)
            {

                var cam = Singleton.Try<GodMode>(s => 
                {
                    float forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
                    float right = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);

                    if (forward != 0 || right != 0) {

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

                /*if (Input.GetMouseButton(0))
                {
                    movementInput = true;

                    if (Input.GetMouseButtonDown(0) == false)
                    {
                        direction += (Input.mousePosition.XY() - previousPos) / ((float)Screen.width) * 10;
                    }

                    previousPos = Input.mousePosition;
                }*/
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

            speed01 = LerpUtils.LerpBySpeed(speed01, movementInput ? 1 : 0, movementInput ? _acceleration : _slowDownSpeed);

            if (!movementInput) 
            {
                direction = LerpUtils.LerpBySpeed(direction, Vector2.zero, 1);
            } 
            

            if (_character) 
            {
                if (direction.magnitude > 1) 
                {
                    direction = direction.normalized;
                }

                var speed = speed01 * _maxSpeed; 

                _character.Move(new Vector3(direction.x * speed, - _gravity, direction.y * speed) * Time.deltaTime );
            }
        }

        public void Inspect()
        {
            pegi.nl();
            "Offset".PegiLabel().edit(ref direction).nl();
            "Character".PegiLabel(60).edit(ref _character).nl();

            if (_character) 
            {
                (_character.isGrounded ? "Grounded" : "In Air").PegiLabel().nl();
            }
        }

        void Reset() 
        {
            _character = GetComponent<CharacterController>();
        }
    }

    [PEGI_Inspector_Override(typeof(TouchscreenMovement))] internal class TouchscreenMovementDrawer : PEGI_Inspector_Override { }

}