using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.RayTracing
{
    public class C_TrailingStretch : MonoBehaviour, IPEGI
    {
        [SerializeField] private Transform _childElement;

        public void Inspect()
        {
            "A child element will be stretched & rotated based on movement. Subchildren of that object should form the tail. For example, a cube should be moved to locatl position (0,0,-0.5). So that only one of it's sides will scale".PegiLabel().WriteHint();
            "Trail Child".PegiLabel(80).Edit(ref _childElement).Nl();

        }


        protected virtual void OnEnable() 
        {
            _previousPosition = transform.position;
        }

        Vector3 _previousPosition;
        Vector3 _directionVector;

        protected virtual void Update() 
        {


            if (_childElement) 
            {
                var newDirection = (transform.position - _previousPosition);

                _directionVector = newDirection; //Vector3.Lerp(_directionVector, newDirection, newDirection.magnitude / (_directionVector.magnitude));

                _previousPosition = transform.position;

                _childElement.LookAt(transform.position + _directionVector, Vector3.up);
                _childElement.localScale = new Vector3(1, 1, 1 + _directionVector.magnitude);

            }
        }
    }

    [PEGI_Inspector_Override(typeof(C_TrailingStretch))] internal class TrailingStretchDrawer : PEGI_Inspector_Override { }
}