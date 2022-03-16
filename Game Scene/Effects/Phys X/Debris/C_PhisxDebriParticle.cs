using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [DisallowMultipleComponent]
    public class C_PhisxDebriParticle : MonoBehaviour, IPEGI
    {
        [SerializeField] private List<Mesh> meshes;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshCollider _meshCollider;
        [SerializeField] private Rigidbody _rigidbody;

        private bool _isPlaying;
        private float lifeTime;


        void OnCollisionEnter(Collision collision)
        {
            float force = 0;

            foreach (var c in collision.contacts) {

                float angle = Mathf.Max(0, Vector3.Dot(c.normal, collision.relativeVelocity));

                if (collision.rigidbody)
                {
                    angle *= collision.rigidbody.mass;
                }
                else
                    angle *= _rigidbody.mass;

                force += angle;
            }

            const float THOLD = 5;

            if (Camera.main.IsInCameraViewArea(transform.position) == false) 
            {
                RemoveToPool();
                return;
            }

            if (force > THOLD)
            {
                Singleton.Try<Pool_SmokeEffects>(s => s.TrySpawn(transform.position, inst => inst.Size = Size));

                if (Size > 0.3f)
                {
                    var count = 1 + Size / 0.3f;

                    for (int i=0; i< count; i++)
                    Singleton.Try<Pool_PhisXDebrisParticles>(s =>
                    {
                        s.TrySpawn(transform.position, el =>
                        {
                            el.Size = 0.1f + 0.1f * Random.value; 
                            el.Push(-_rigidbody.velocity.normalized, 1, 1, 50);
                        });
                    });
                } 

                RemoveToPool();
            }
        }

        public float Size 
        {
            get => transform.localScale.x;
            set 
            {
                transform.localScale = Vector3.one * value;
                _rigidbody.mass = value;
            }
        }

        void Reset() 
        {
            _meshFilter = GetComponent<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Push (Vector3 pushVector, float pushForce, float pushRandomness, float torqueForce) 
        {
            _meshFilter.sharedMesh = meshes.GetRandom();
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;

            transform.position += 0.6f * Size * pushVector.normalized;

            _isPlaying = true;
            lifeTime = 0;
            _rigidbody.AddForce((pushVector.normalized + Random.insideUnitSphere * pushRandomness).normalized * pushForce, ForceMode.VelocityChange);
            _rigidbody.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);
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
            if (_isPlaying)
            {
                lifeTime += Time.deltaTime;

                var s = Size;

                float totalLifetime = 20f * s;
                float startFade = 10f * s;

                if (lifeTime >= totalLifetime)
                {
                    RemoveToPool();
                } else if (lifeTime > startFade) 
                {
                    float timeTillFade = totalLifetime - lifeTime;
                  
                    Size = LerpUtils.LerpBySpeed(s, 0, s/(timeTillFade+0.01f));
                }
            }
        }

        public void RemoveToPool()
        {
            _isPlaying = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            Pool_PhisXDebrisParticles.ReturnToPool(this);

        }

        private float _testPushAmount = 10;
        private float _testRandomDirection = 50;
        private float _testTorqueAmount = 50;
        public void Inspect()
        {
            if (Application.isPlaying == false) 
            {
                pegi.TryDefaultInspect(this);
                return;
            }

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

    [PEGI_Inspector_Override(typeof(C_PhisxDebriParticle))] internal class C_PhisxDebriParticleDrawer : PEGI_Inspector_Override { }
}