using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [DisallowMultipleComponent]
    public class C_PhisxDebriParticle : MonoBehaviour, IPEGI
    {
        [SerializeField] private List<Mesh> meshes;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshCollider _meshCollider;
        [SerializeField] private Rigidbody _rigidbody;

        [SerializeField] private GameObject _shadow;
        [SerializeField] private GameObject _light;

        private bool _isPlaying;
        private float _heatEmission;
        private float lifeTime;
        private LogicWrappers.DeltaPositionSegments _smokeSpawning = new LogicWrappers.DeltaPositionSegments();

        void OnCollisionEnter(Collision collision)
        {
            float force = 0;

            foreach (var c in collision.contacts) 
            {

                float angle = Mathf.Max(0, Vector3.Dot(c.normal, collision.relativeVelocity));

                if (collision.rigidbody)
                {
                    angle *= collision.rigidbody.mass;
                }
                else
                    angle *= _rigidbody.mass;

                force += angle;
            }

            if (Camera.main.IsInCameraViewArea(transform.position) == false) 
            {
                RemoveToPool();
                return;
            }

            const float THOLD = 3;

            if (Size > 0.3f)
            {
                Singleton.Try<Pool_AnimatedSmokeOneShoot>(s => s.TrySpawn(transform.position, inst => { inst.transform.localScale = inst.transform.localScale * 10 * Size; }));

                if (force > THOLD)
                {
                    //Singleton.Try<Pool_SmokeEffects>(s => s.TrySpawn(transform.position, inst => inst.Size = Size));

                    //   Singleton.Try<Pool_AnimatedSmokeOneShoot>(s => s.TrySpawn(transform.position, inst => { inst.transform.localScale = inst.transform.localScale * 5 * Size; }));


                    var count = 1 + Size / 0.3f;

                    for (int i = 0; i < count; i++)
                        Singleton.Try<Pool_PhisXDebrisParticles>(s =>
                        {
                            s.TrySpawn(transform.position, el =>
                            {
                                el.Reset(size: 0.1f + 0.1f * Random.value);
                                el.Push(-_rigidbody.velocity.normalized, 1, 1, 50);
                            });
                        });


                   
                }
            }

            if (force > THOLD)
            {
                RemoveToPool();
            }
           /* else if (force > THOLD * 0.25f)
            {
                Singleton.Try<Pool_ECS_HeatSmoke>(s => s.TrySpawn(worldPosition: transform.position));
            }*/
        }

        public float Size 
        {
            get => transform.localScale.x;
            set 
            {
                transform.localScale = Vector3.one * value;
               
            }
        }

        void Reset() 
        {
            _meshFilter = GetComponent<MeshFilter>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Reset(float size) 
        {
            _smokeSpawning.Reset(transform.position);
            Size = size;
            _shadow.SetActive(Size > 0.5f);
            _light.SetActive(false);
        }

        public void Push (Vector3 pushVector, float pushForce, float pushRandomness, float torqueForce, float heat = 0) 
        {
            _heatEmission = heat;
            _light.gameObject.SetActive(heat > 0);
            _meshFilter.sharedMesh = meshes.GetRandom();
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;

            transform.position += 0.6f * Size * pushVector.normalized;

            _isPlaying = true;
            lifeTime = 0;
            _rigidbody.AddForce((pushVector.normalized + Random.insideUnitSphere * pushRandomness).normalized * pushForce, ForceMode.VelocityChange);
            _rigidbody.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);
            _rigidbody.mass = Size;

           
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

                if (s > 0.2f && _heatEmission > 0 &&  _smokeSpawning.TryGetSegments(transform.position, delta: 0.5f, out Vector3[] smokePoints))
                {
                    if (Camera.main.IsInCameraViewArea(transform.position))
                    {

                      
                        
                        Singleton.Try<Pool_ECS_HeatSmoke>(s =>
                        {
                            for (int i = 0; i < smokePoints.Length; i++)
                            {
                                if (!s.TrySpawn(worldPosition: smokePoints[i] + Random.insideUnitSphere * 0.25f, out C_ECS_HeatSmoke inst)) 
                                    break;

                                inst.AddHeat(20);

                                _heatEmission--;

                                if (_heatEmission <= 0)
                                {
                                    _light.gameObject.SetActive(false);
                                    break;
                                }
                            }
                        });
                    }
                }

                float totalLifetime = 20f * s;
                float startFade = 10f * s;

                if (lifeTime >= totalLifetime)
                {
                    RemoveToPool();
                } else if (lifeTime > startFade) 
                {
                    float timeTillFade = totalLifetime - lifeTime;
                  
                    Size = LerpUtils.LerpBySpeed(s, 0, s/(timeTillFade+0.01f), unscaledTime: false);
                }
            }
        }

        public void RemoveToPool()
        {
            _isPlaying = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            Pool.Return(this);

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