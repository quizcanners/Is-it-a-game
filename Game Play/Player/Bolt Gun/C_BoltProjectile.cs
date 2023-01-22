using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [DisallowMultipleComponent]
    public class C_BoltProjectile : MonoBehaviour, IPEGI
    {
        private C_AttachmentPositionProxy _impactInstance;

        private float emitBloodFromImpact = 0;
        private Vector3 _velocity;
        private bool _isFlying;
        private readonly Gate.UnityTimeScaled _maxFlyTime = new();
        private readonly Gate.UnityTimeScaled _maxImpactAttachTime = new();

        private PlayerGun_BoltGun _config;


        Pool_BoltProjectiles MGMT => Singleton.Get<Pool_BoltProjectiles>();

        public void Restart(Vector3 velocity, PlayerGun_BoltGun config) 
        {
            transform.localScale = Vector3.one * 3;
           _config = config;
            _velocity = velocity;
            _isFlying = true;
            emitBloodFromImpact = 0;
            _maxFlyTime.Update();

            Update();
        }

        private void OnDisable()
        {
            if (_impactInstance)
                _impactInstance.gameObject.DestroyWhatever();
        }

        void Update() 
        {
            if (_isFlying)
            {
                if (_maxFlyTime.TryUpdateIfTimePassed(secondsPassed: 10)) 
                {
                    MGMT.ReturnToPool(this);
                    return;
                }

                Vector3 delta = _velocity * Time.deltaTime;

                if (delta.magnitude > 0)
                {
                    transform.rotation = Quaternion.LookRotation(delta);
                }

                Singleton.Try<Singleton_ChornobaivkaController>(s =>
                {
                    if (s.CastHardSurface(new Ray(transform.position, _velocity), out RaycastHit hit, maxDistance: delta.magnitude * 1.25f))
                    {
                        bool visibleByCamera = Camera.main.IsInCameraViewArea(hit.point);

                        _isFlying = false;
                        transform.localScale = Vector3.one;

                        Vector3 damagePosition = hit.point;
                        bool dismemberment = false;

                        var proxy = hit.transform.gameObject.GetComponentInParent<CreatureProxy_Base>();

                        bool pierced = false;

                        if (proxy && proxy.Parent) 
                        {
                            var parent = proxy.Parent;

                            if (parent.TrySetProxy(C_Monster_Data.Proxy.Detailed))
                            {
                                var monster = parent.proxy_detailed;

                                monster.TryGetNearestRigidbody(hit.point, out var rbb, out var _);

                                _impactInstance = C_AttachmentPositionProxy.AttachTo(rbb ? rbb.transform : hit.transform, hit.point, transform.rotation, "Bolt ATTACH");
                                _maxImpactAttachTime.Update();

                                monster.ApplyImpact(hit, ref pierced, onDismemberment: newPos =>
                                {
                                    damagePosition = newPos;
                                    dismemberment = true;
                                });

                                if (parent.TryTakeHit(_config.WeaponAttack, RollInfluence.Advantage, C_MonsterDetailed.LimbsControllerState.Animation))
                                {
                                    if (visibleByCamera)
                                    {
                                        switch (monster.LimbsState)
                                        {
                                            case C_MonsterDetailed.LimbsControllerState.Ragdoll:
                                                if (monster.TryGetNearestRigidbody(hit.point, out var rb, out float dist))
                                                {
                                                    rb.AddForce(_velocity.normalized * 25, mode: ForceMode.Impulse);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }

                            emitBloodFromImpact = 0.5f;
                            Game.Enums.UiSoundEffects.BodyImpact.PlayOneShotAt(proxy.transform.position, clipVolume: 2);

                        }
                        else 
                        {
                            Game.Enums.UiSoundEffects.DefaultSurfaceImpact.PlayOneShotAt(hit.point);

                            if (!visibleByCamera)
                                return;

                            Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawn(hit.point, onInstanciate: l => l.SetSize(25f)));

                        }

                        ChornobaivkaUtils.PaintDamageOnImpact_IfVisible(hit, _config.brushConfig.brush,
                                direction: _velocity.normalized, dismemberment: dismemberment, damagePosition: damagePosition);
                    }
                });

                transform.position += delta;

                _velocity += Physics.gravity * Time.deltaTime;
            } else 
            {
                if (_maxImpactAttachTime.TryUpdateIfTimePassed(10)) 
                {
                    MGMT.ReturnToPool(this);
                    return;
                }
            }
        }

        void LateUpdate() 
        {
            if (!_isFlying) 
            {
                if (!_impactInstance)
                    MGMT.ReturnToPool(this);
                else 
                {
                    var tf = _impactInstance.transform;
                    transform.position = tf.position;
                    transform.rotation = tf.rotation;

                    if (emitBloodFromImpact > 0) 
                    {
                        if (Camera.main.IsInCameraViewArea(transform.position))
                        {
                            Singleton.Try<Singleton_ZibraLiquidsBlood>(sb =>
                            {
                                sb.TryEmitFrom(_impactInstance.transform, Vector3.back, amountFraction: emitBloodFromImpact * 3);
                            });

                            emitBloodFromImpact -= Time.deltaTime;
                        } 
                        else
                            MGMT.ReturnToPool(this);
                    }
                }
            }
        }

        public void Inspect()
        {

        }
    }
    [PEGI_Inspector_Override(typeof(C_BoltProjectile))] 
    internal class C_BoltProjectileDrawer : PEGI_Inspector_Override { }
}