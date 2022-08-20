using Dungeons_and_Dragons;
using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerGun_RocketLauncher : IPEGI
    {

        [SerializeField] private RayfireGun gun;
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;
        public PlaytimePainter_BrushConfigScriptableObject fleshImpactBrushConfig;
        public SavingThrow savingThrow = new() { DC = 17, Score = AbilityScore.Dexterity };
        public Damage Damage = new() { DamageDice = new List<Dice> { Dice.D10 }, DamageType = DamageType.Fire };
        [SerializeField] private float _pushForce = 200;
        [SerializeField] private float _explosionRadius = 8;

        public void Explosion(RaycastHit hit, Vector3 projectileVelocity, State state)
        {
            Vector3 origin = hit.point;

            int killedMonsters = 0;
            bool splatterMonsters = false;
            var enm = Singleton.Get<Pool_MonstersController>();

            if (enm)
            {

                int maxPaints = 2;

                foreach (C_MonsterEnemy m in enm)
                {
                    var pos = m.GetActivePosition();
                    var pushDirection = pos - origin;
                    var dist = pushDirection.magnitude;

                    float GetPushForce() => 2 * (_explosionRadius - dist) / _explosionRadius;

                    if (dist < _explosionRadius)
                    {
                        if (m.IsAlive)
                        {
                            killedMonsters++;

                            if (dist < _explosionRadius * 0.3f)
                            {
                                m.Disintegrate(); // pushDirection.normalized, pushForce01: GetPushForce());

                                Singleton.Try<Pool_VolumetricBlood>(s => s.TrySpawnFromHit(hit, pushDirection, out BFX_BloodController controller, size: 4, impactDecal: false));

                                Singleton.Try<Pool_VolumetricBlood>(s => s.TrySpawnFromHit(hit, -pushDirection, out BFX_BloodController controller, size: 3, impactDecal: false));

                                splatterMonsters = true;
                            }
                            else
                            {
                               
                                var randomLimb = m.GetRandomCollider();

                                var fragmentOrigin = hit.point + hit.normal * 0.2f;

                                var ray = QcUnity.RaySegment(fragmentOrigin, randomLimb.transform.position, out float distance);// new Ray(fragmentOrigin, direction: randomLimb.transform.position);


                                if (maxPaints > 0)
                                {
                                    maxPaints--;

                                    Singleton.Try<Singleton_ChornobaivkaController>(s =>
                                    {

                                        if (s.CastHardSurface(ray, out var limbHit, maxDistance: distance))
                                        {
                                            C_MonsterEnemy hitMonster = limbHit.transform.GetComponentInParent<C_MonsterEnemy>();
                                            if (hitMonster && hitMonster == m)
                                            {
                                                PainDamage(limbHit, fleshImpactBrushConfig);

                                            }
                                        }
                                    });
                                }

                                m.DropRigid();
                              
                            }
                        }
                        else if (dist < _explosionRadius * 0.6f)
                        {
                            if (m.LimbsState != C_MonsterEnemy.LimbsControllerState.Giblets && m.LimbsState != C_MonsterEnemy.LimbsControllerState.Disintegrating)
                                m.Giblets(pushDirection.normalized, pushForce01: GetPushForce());
                        }

                        if (m.LimbsState == C_MonsterEnemy.LimbsControllerState.Ragdoll)
                        {
                            m.Push(force: _pushForce * 20, origin: hit.point - Vector3.up*2, radius: _explosionRadius);
                        }
                    }
                }
            }

            const float volume = 0.5f;

            if (splatterMonsters)
            {
                Game.Enums.SoundEffects.Explosion_Gory.PlayOneShotAt(hit.point, clipVolume: volume);
            }
            else
                Singleton.Try<Singleton_CameraOperatorGodMode>(
                    onFound: cam => {
                            if ((cam.transform.position - hit.point).magnitude < _explosionRadius)
                                Game.Enums.SoundEffects.Explosion_Near.PlayOneShotAt(hit.point, clipVolume: volume);
                            else 
                                Game.Enums.SoundEffects.Explosion.PlayOneShotAt(hit.point, clipVolume: volume);
                        }, 
                    onFailed: () => Game.Enums.SoundEffects.Explosion.PlayOneShotAt(hit.point, clipVolume: volume));


            if (!Camera.main.IsInCameraViewArea(hit.point))
                return;

            latestState = state;
            _explosionsLeft.ResetCountDown(10);
            _origin = origin;
            iteration = 0;




            if (state.Gun)
            {
                var cmp = hit.transform.gameObject.GetComponentInParent<C_RayFireRespawn>();
                if (cmp)
                    cmp.SetDamaged(true);

                state.Gun.Shoot(origin - projectileVelocity, projectileVelocity);
            }

            Singleton.Try<Pool_ImpactLightsController>(s =>
            {
                if (s.TrySpawnIfVisible(origin, out var light))
                    light.SetSize(128);
            });

            if (Pool.TrySpawnIfVisible<C_SmokeEffectOnImpact>(origin, out var smoke))
                smoke.PlayAnimateFromDot(2f);

            Singleton.Try<Pool_PhisXDebrisParticles>(s =>
            {
                s.PushFromExplosion(hit.point, force: _pushForce, radius: 25);

                int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 30);

                var reflected = Vector3.Reflect(-projectileVelocity.normalized, hit.normal);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawnIfVisible(hit.point, out var debri))
                    {
                        var big = UnityEngine.Random.value;

                        debri.Reset(size: 0.02f + 0.5f * big * big);

                        var direction = Vector3.Lerp(reflected, hit.normal, 0.5f + big * 0.5f);

                        debri.Push(pushVector: direction, pushForce: 10.5f, pushRandomness: debri.Size * 15, torqueForce: 540, heat: big * 35);
                    }
                    else break;
                }
            });

            Singleton.Try<Pool_PhisXEmissiveParticles>(s =>
            {
                s.PushFromExplosion(hit.point, force: 5, 20);

                int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 10);

                var reflected = Vector3.Reflect(-projectileVelocity.normalized, hit.normal);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawnIfVisible(hit.point, out var debri))
                    {
                        var big = UnityEngine.Random.value;

                        debri.Size = (0.2f + big);

                        var direction = Vector3.Lerp(reflected, hit.normal, 0.5f + big * 0.5f);

                        debri.Push(pushVector: direction, pushForce: debri.Size * 3f, pushRandomness: 2f, torqueForce: 0);
                    }
                    else break;
                }
            });

            Singleton.Try<Singleton_SDFPhisics_TinyEcs>(s =>
            {
                s.World.CreateEntity().AddComponent((ref ParticlePhisics.UpwardImpulse e) => 
                { 
                    e.EnergyLeft = 10; 
                    e.Position = hit.point; 
                });
            });

            Singleton.Try<Pool_ECS_HeatSmoke>(p =>
            {
                for (int i = 0; i < 5; i++)
                {
                    if (!p.TrySpawn(hit.point + UnityEngine.Random.insideUnitCircle.ToVector3XZ() * 3.5f))
                        break;
                }
            });


            PainDamage(hit, brushConfig);
        }


        private void PainDamage(RaycastHit hit, PlaytimePainter_BrushConfigScriptableObject brush) 
        {
            var receivers = hit.transform.GetComponentsInParent<C_PaintingReceiver>();

            if (receivers.Length > 0)
            {
                C_PaintingReceiver receiver = receivers.GetByHit(hit, out int subMesh);

                if (receiver.GetTexture() is RenderTexture)
                {
                    var stroke = receiver.CreateStroke(hit);
                    BrushTypes.Sphere.Paint(receiver.CreatePaintCommandForSphereBrush(stroke, brush.brush, subMesh));
                }
            }
        }

        [SerializeField] private pegi.EnterExitContext context = new();
        public void Inspect()
        {
            using (context.StartContext())
            {
                "Direct Impact Brush".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
                "Shrapnel Impact Brush".PegiLabel().Edit_Enter_Inspect(ref fleshImpactBrushConfig).Nl();
            }
        }


        private State latestState;
        private Vector3 _origin;
        private LogicWrappers.CountDown _explosionsLeft = new();
        private Gate.UnityTimeScaled _explosionDynamics = new();
        private int iteration;

        public void UpdateExplosions() 
        {
            if (!_explosionsLeft.IsFinished && _explosionDynamics.TryUpdateIfTimePassed(0.02f)) 
            {
                iteration++;
                _explosionsLeft.RemoveOne();

                if (latestState.Gun)
                {
                    var dir = UnityEngine.Random.insideUnitSphere;
                    dir.y = Mathf.Abs(dir.y)*0.25f;

                    if (Physics.Raycast(_origin, dir, out var hit, maxDistance: (iteration + 1) * 0.5f * _explosionRadius)) 
                    {
                        var cmp = hit.transform.gameObject.GetComponentInParent<C_RayFireRespawn>();

                        if (cmp)
                        {
                            cmp.SetDamaged(true); 
                            latestState.Gun.Shoot(_origin, dir);
                        }
                    }
                }


                Singleton.Try<Pool_ECS_HeatSmoke>(p =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (!p.TrySpawn(_origin + UnityEngine.Random.insideUnitCircle.ToVector3XZ() * (1f + iteration) * 0.4f))
                            break;
                    }
                });

                /*
                Singleton.Try<Pool_PhisXEmissiveParticles>(s =>
                {

                    int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 10);

         
                    for (int i = 0; i < count; i++)
                    {
                        if (s.TrySpawnIfVisible(_origin, out var debri))
                        {
                            var big = UnityEngine.Random.value;

                            debri.Size = (0.2f + big);


                            debri.Push(pushVector: Vector3.up, pushForce: debri.Size * 7f, pushRandomness: 2f, torqueForce: 0);
                        }
                        else break;
                    }
                });*/


            }
        }


        [Serializable]
        public class State : IPEGI
        {
            [SerializeField] public RayfireGun Gun;

            public void Inspect()
            {
                "Gun".PegiLabel(60).Edit(ref Gun).Nl();
            }
        }


    }

    
}
