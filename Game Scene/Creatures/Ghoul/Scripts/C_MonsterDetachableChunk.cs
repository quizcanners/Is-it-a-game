using UnityEngine;


namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDetachableChunk : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private Collider _collider;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool initialCollider;

        public bool Detached 
        {
            set 
            {
                _rigidBody.isKinematic = !value;
                _collider.enabled = value ? true : initialCollider;

                if (!value) 
                {
                    transform.localPosition = _initialPosition;
                    transform.localRotation = _initialRotation;
                }
            }
        }

        private void Reset()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        private void Start()
        {
            _initialPosition = transform.localPosition;
            _initialRotation = transform.localRotation;
            initialCollider = _collider.enabled;
        }
    }
}