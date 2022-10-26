using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    [ExecuteAlways]
    public class C_SDFTrace : MonoBehaviour, IPEGI
    {
        [SerializeField] private Transform _lineRotation;
        [SerializeField] private Transform _lineLength;
        [SerializeField] private float _thickness = 0.25f;


        private Vector3 _startPosition;
        [SerializeField] public float Speed = 100;
        private bool _isAnimating;

        public void ResetTrail() 
        {
            _startPosition = transform.position;
            LateUpdate();
        }

        public void RestartOnParent(Transform parent)
        {
            gameObject.SetActive(true);
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            ResetTrail();
            _isAnimating = true;
        }

 

        void OnEnable() 
        {
            ResetTrail();
        }

        void Reset() 
        {
            _lineRotation = transform.childCount > 0 ? transform.GetChild(0) : null;
        }

        int motionlessFrames = 0;

        void LateUpdate() 
        {
            Vector3 target = transform.position;

            _startPosition = LerpUtils.LerpBySpeed(_startPosition, target, Speed, unscaledTime: false);

            var length = Vector3.Distance(_startPosition, target);

            Vector3 raisedOrigin = _startPosition; // + Vector3.up;

            _lineRotation.transform.position = (raisedOrigin + transform.position) * 0.5f;
            _lineRotation.LookAt(transform, Vector3.up);
            _lineLength.localScale = new Vector3(_thickness, Mathf.Max(0.5f, length * 0.5f), _thickness);

            if (_isAnimating)
            {
                if (length < 0.05f)
                {
                    motionlessFrames++;
                    if (motionlessFrames > 2)
                    {
                        gameObject.SetActive(false);
                        _isAnimating = false;
                    }
                } else
                    motionlessFrames = 0;
            }
               

            //_line.Rotate(Quaternion.Euler(90,0,0));
        }

        public void Inspect()
        {
            pegi.Click(ResetTrail).Nl();

            "Speed".PegiLabel().Edit(ref Speed).Nl();

            "Length: {0}".F(Vector3.Distance(_startPosition, transform.position).ToString()).PegiLabel().Nl();

        }
    }

    [PEGI_Inspector_Override(typeof(C_SDFTrace))] internal class C_SDFTraceDrawer : PEGI_Inspector_Override { }
}