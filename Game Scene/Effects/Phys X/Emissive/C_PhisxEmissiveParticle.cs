using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [DisallowMultipleComponent]
    [SelectionBase]
    public class C_PhisxEmissiveParticle : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _2dLight;
        [SerializeField] private Transform _billboard;

        private bool _isPlaying;
        private float lifeTime;

        private float _size = -1;

        public float Size 
        {
            get => _size;// transform.localScale.x;
            set 
            {
                _size = value;
                transform.localScale = Vector3.one * (_size < 1 ? _size * _size : _size);
            }
        }

        void Reset() 
        {
            _meshFilter = GetComponent<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Push (Vector3 pushVector, float pushForce, float pushRandomness, float torqueForce) 
        {
            transform.position += 0.6f * Size * pushVector.normalized;

            _isPlaying = true;
            lifeTime = 0;
            _rigidbody.velocity = Vector3.zero;
            
            _rigidbody.AddForce((pushVector.normalized + Random.insideUnitSphere * pushRandomness).normalized * pushForce, ForceMode.VelocityChange);
            _rigidbody.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);
            _rigidbody.mass = _size * 0.1f;
        }

        public void Explosion(Vector3 origin, float force, float radius) 
        {
            _rigidbody.AddExplosionForce(
                explosionForce: force, 
                explosionPosition: origin, 
                explosionRadius: radius, 
                upwardsModifier: 1.5f);
        }

        void Update() 
        {

            if (_2dLight)
            {
                _2dLight.rotation = Quaternion.Euler(90, 0, 0);
            }

            if (_billboard)
                _billboard.LookAt(Camera.main.transform);

            if (_isPlaying)
            {
                lifeTime += Time.deltaTime;

                var s = Size;

                float totalLifetime = 6f * s;

                if (lifeTime >= totalLifetime)
                {
                    _isPlaying = false;
                    Pool.Return(this);
                } else
                {
                    float timeTillFade = totalLifetime - lifeTime;
                    Size = LerpUtils.LerpBySpeed(s, 0, s/(timeTillFade+0.01f), unscaledTime: false);
                }
            }
        }

        private float _testPushAmount = 10;
        private float _testRandomDirection = 50;
        private float _testTorqueAmount = 50;
        public void Inspect()
        {
            var s = Size;
            "Size".PegiLabel(40).Edit(ref s, 0.01f, 1f).Nl().OnChanged(()=> Size = s);
            "Push Amount".PegiLabel(90).Edit(ref _testPushAmount).Nl();
            "Push Random".PegiLabel(90).Edit(ref _testRandomDirection).Nl();
            "Torqu Amount".PegiLabel(90).Edit(ref _testTorqueAmount).Nl();

            "Push Up".PegiLabel().Click(() =>
            {
                Push(Vector3.up, _testPushAmount, _testRandomDirection, _testTorqueAmount);
            }).Nl();

            if (_meshFilter && _meshFilter.sharedMesh) 
            {
                var meshName = "Debris " + _meshFilter.sharedMesh.name;
                if (gameObject.name.Equals(meshName) == false && "Set Name".PegiLabel().Click().Nl())
                    gameObject.name = meshName;
            }

        }
    }

    [PEGI_Inspector_Override(typeof(C_PhisxEmissiveParticle))] internal class C_PhisxEmissiveParticleDrawer : PEGI_Inspector_Override { }
}