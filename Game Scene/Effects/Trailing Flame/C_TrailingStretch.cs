using QuizCanners.Inspect;
using QuizCanners.Lerp;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_TrailingStretch : MonoBehaviour, IPEGI
    {
        [SerializeField] private Transform _childElement;
        [SerializeField] private float speed = 150;
        [SerializeField] private TrailRenderer trail;

        [NonSerialized] private bool _animating;
        [NonSerialized] private Vector3 _target;

        Vector3 _previousPosition;
        Vector3 _directionVector;

        public void FlyTo(Vector3 position) 
        {
            _animating = true;
            _target = position;
            Reboot();
        }

        public void Reboot() 
        {
            _previousPosition = transform.position;
            _directionVector = Vector3.zero;
            trail.Clear();
        }

        protected virtual void OnEnable() 
        {
            Reboot();
        }

        protected virtual void LateUpdate() 
        {
            if (_animating) 
            {
                if (Vector3.Distance(transform.position, _target) < 0.1f)
                {
                    Pool_TrailEffectController.ReturnToPool(this);
                }

                transform.position = LerpUtils.LerpBySpeed(transform.position, _target, speed, unscaledTime: false);
            }

            if (_childElement) 
            {
                var newDirection = (transform.position - _previousPosition);
                _directionVector = newDirection; 
                _previousPosition = transform.position;

                _childElement.LookAt(transform.position + _directionVector, Vector3.up);
                _childElement.localScale = new Vector3(1, 1, 1 + _directionVector.magnitude);
            }
        }


        public void Inspect()
        {
            "A child element will be stretched & rotated based on movement. Subchildren of that object should form the tail. For example, a cube should be moved to locatl position (0,0,-0.5). So that only one of it's sides will scale".PegiLabel().WriteHint();
            "Trail Child".PegiLabel(80).Edit(ref _childElement).Nl();
            "Trail Renderer".PegiLabel(80).Edit_IfNull(ref trail, gameObject).Nl();
        }

    }

    [PEGI_Inspector_Override(typeof(C_TrailingStretch))] internal class TrailingStretchDrawer : PEGI_Inspector_Override { }
}