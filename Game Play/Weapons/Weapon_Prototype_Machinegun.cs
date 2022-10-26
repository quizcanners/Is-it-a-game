using Dungeons_and_Dragons;
using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class Weapon_Prototype_Machinegun :  IPEGI, INeedAttention
    {
        [SerializeField] private PlaytimePainter_BrushConfigScriptableObject brushConfig;
        [SerializeField] private Attack WeaponAttack = new(name: "Gun", isRange: true, attackBonus: 3,
             new Damage()
             {
                 DamageBonus = 2,
                 DamageDice = new List<Dice> { Dice.D10 },
                 DamageType = DamageType.Piercing
             });

        [SerializeField] private float MaxDistance = 2000;

        public void Shoot(Vector3 from, Vector3 target, State state, out Vector3 actualTarget, out List<C_Monster_Data> monsters)
        {
            monsters = new List<C_Monster_Data>();

            Game.Enums.UiSoundEffects.Shot.PlayOneShotAt(from, clipVolume: 3);

            var spread = UnityEngine.Random.insideUnitSphere * state.WeaponKick;

            actualTarget = target;

            Singleton_ChornobaivkaController mgmt = Singleton.Get<Singleton_ChornobaivkaController>();
            if (!mgmt)
            {
                QcLog.ChillLogger.LogErrorOnce("{0} not found".F(nameof(Singleton_ChornobaivkaController)), key: "NoChrnb");
                return;
            }

            if (state.MuzzleFlash && Camera.main.IsInCameraViewArea(from))
                state.MuzzleFlash.Emit();

            Ray ray = new(from + spread, target - from);

            RaycastHit firstHit;

            if (mgmt.CastHardSurface(ray, out firstHit) || mgmt.CastPierce(ray, out firstHit))
            {
                Vector3 direction = (firstHit.point - from).normalized;

                actualTarget = firstHit.point;

                Singleton.Try<Pool_TrailEffectController>(s =>
                {
                    s.TrySpawn(from, out var trace);
                    trace.FlyTo(firstHit.point);
                });

                Singleton.Try<Singleton_CameraOperatorGodMode>(c =>
                {
                    var point = QcMath.GetClosestPointOnALine(lineA: from, lineB: firstHit.point, point: c.transform.position);
                    Game.Enums.UiSoundEffects.BulletFlyBy.PlayOneShotAt(point, clipVolume: 0.5f);
                });

                state.WeaponKick = Mathf.Clamp01(state.WeaponKick + 0.3f);

                ProcessHit(firstHit, ray.origin, direction, out bool pierced, state, monsters);

                int maxLine = 10;

                RaycastHit latestHit = firstHit;

                while (pierced && maxLine > 0) 
                {
                    maxLine--;
                    pierced = false;

                    var outRay = new Ray(origin: latestHit.point, direction: direction);

                    if (mgmt.CastHardSurface(outRay, out latestHit)) 
                    {
                        ProcessHit(latestHit, ray.origin, direction, out pierced, state, monsters);
                    }
                }

                if (mgmt.CastPierce(ray, firstHit, out var softHit)) 
                {
                    ProcessHit(softHit, ray.origin, direction, out pierced, state, monsters);
                }

            }
        }

        private void ProcessHit(RaycastHit hit, Vector3 origin, Vector3 direction, out bool pierced, State state, List<C_Monster_Data> monsters) 
        {
            bool visibleByCamera = Camera.main.IsInCameraViewArea(hit.point);
            bool isNear = Camera.main.IsInCameraViewArea(hit.point, maxDistance: 30);
            pierced = false;

            Vector3 damagePosition = hit.point;

            var proxy = hit.transform.gameObject.GetComponentInParent<CreatureProxy_Base>();

            bool dismemberment = false;

            if (proxy)
            {
                var parent = proxy.parent;

                proxy.ApplyImpact(hit, ref pierced, 
                    onDismemberment: (detach) => 
                {
                    damagePosition = detach;
                    dismemberment = true;
                });

                if (parent)
                {
                    if (parent.TryTakeHit(WeaponAttack, RollInfluence.Advantage, C_MonsterDetailed.LimbsControllerState.Animation))
                    {
                        ApplyDamageToProxy();
                        pierced = !parent.IsAlive;
                        if (!pierced)
                            monsters.Add(parent);

                    } else { 
                        Game.Enums.UiSoundEffects.ArmorImpact.PlayOneShotAt(hit.point);
                        if (visibleByCamera)
                            Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawnIfVisible(hit.point, onInstanciate: l => l.SetSize(10f)));
                    }
                }
                else
                {
                    ApplyDamageToProxy();
                }

                void ApplyDamageToProxy()
                {
                    if (visibleByCamera)
                    {
                        if (isNear)
                        {
                            SpawnBlood(hit, direction);
                        }
                        else
                        {
                            Singleton.Try<Singleton_ZibraLiquidsBlood>(sb =>
                            {
                                if (!sb.TryEmitFrom(hit))
                                    SpawnBlood(hit, direction);

                            }, onFailed: () => SpawnBlood(hit, direction));
                        }

                        var detailed = proxy as C_MonsterDetailed;

                        if (detailed)
                        {
                            if (detailed)
                                detailed.ShowDamage = true;

                            if (isNear && detailed)
                            {
                                switch (detailed.LimbsState)
                                {
                                    case C_MonsterDetailed.LimbsControllerState.Ragdoll:
                                        if (detailed.TryGetNearestRigidbody(hit.point, out var rb, out float dist))
                                        {
                                            rb.AddForce((Vector3.up + UnityEngine.Random.insideUnitSphere) * 25, mode: ForceMode.Impulse);
                                            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 15, ForceMode.Impulse);
                                            // rb.AddForce(direction * 50, mode: ForceMode.Impulse);

                                            detailed.OnAnyDamage();
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                detailed.OnAnyDamage();
                            }
                        }
                        else if (parent.IsAlive == false)
                        {
                            var instanced = proxy as C_MonsterInstanced;

                            if (instanced)
                                instanced.Giblets();
                        }
                    }

                    if (isNear)
                        Game.Enums.UiSoundEffects.BodyImpact.PlayOneShotAt(hit.point, clipVolume: 2);
                }

            }
            else
            {
                Game.Enums.UiSoundEffects.DefaultSurfaceImpact.PlayOneShotAt(hit.point);

                if (!visibleByCamera)
                    return;

                if (isNear)
                    Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawn(hit.point, onInstanciate: l => l.SetSize(25f)));

                if (state.Gun)
                {
                    var cmp = hit.transform.gameObject.GetComponentInParent<C_RayFireRespawn>();
                    if (cmp)
                    {
                        cmp.SetDamaged(true);
                        state.Gun.Shoot(origin, direction);
                        return;
                    } else 
                    {
                        state.Gun.Shoot(origin, direction);
                    }
                }

                if (isNear)
                {
                    Singleton.Try<Pool_AnimatedExplosionOneShoot>(s => { s.TrySpawn(hit.point); });

                    Singleton.Try<Pool_SmokeEffects>(s => s.TrySpawn(hit.point, out _));

                    Singleton.Try<Pool_PhisXDebrisParticles>(s =>
                    {
                        s.PushFromExplosion(hit.point, force: 20, 2);

                        int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 10);

                        var reflected = Vector3.Reflect(direction, hit.normal);

                        for (int i = 0; i < count; i++)
                        {
                            if (s.TrySpawnIfVisible(hit.point, out var debri))
                            {
                                var big = UnityEngine.Random.value;
                                debri.Reset(size: Mathf.Pow(0.1f + 0.3f * big, 2));
                                var dir = Vector3.Lerp(reflected, hit.normal, 0.5f + big * 0.5f);
                                debri.Push(pushVector: dir, pushForce: 0.5f, pushRandomness: debri.Size * 15, torqueForce: 140);
                            }
                            else break;
                        }
                    });
                }

                Singleton.Try<Pool_PhisXEmissiveParticles>(s =>
                {
                    s.PushFromExplosion(hit.point, force: 1, 2);

                    int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 4);

                    var reflected = Vector3.Reflect(direction, hit.normal);

                    for (int i = 0; i < count; i++)
                    {
                        if (s.TrySpawnIfVisible(hit.point, out var debri))
                        {
                            var big = UnityEngine.Random.value;

                            debri.Size = (big * 0.5f);

                            var dir = Vector3.Lerp(reflected, hit.normal, big);

                            debri.Push(pushVector: dir, pushForce: debri.Size * 22f, pushRandomness: 0.1f, torqueForce: 0);
                        }
                        else break;
                    }
                });
            }


            ChornobaivkaUtils.PaintDamageOnImpact_IfVisible(hit, brushConfig.brush, 
                direction: direction, dismemberment: dismemberment, damagePosition: damagePosition);
        }

        private void SpawnBlood(RaycastHit hit, Vector3 direction)
        {

            Pool.TrySpawn<C_BloodSquirt>(hit.point, inst => 
            {
                inst.transform.LookAt(hit.point - direction, Vector3.up);
            });

            var poolOfBlood = Singleton.Get<Pool_BloodParticlesController>();

            if (poolOfBlood)
            {

                int bloodParticles = Mathf.RoundToInt(Mathf.Max(1, poolOfBlood.VacancyPortion * 2));

                Vector3 from = hit.point;// + direction * 0.2f;

                for (int i = 0; i < bloodParticles; i++)
                {
                    if (poolOfBlood.TrySpawn(hit.point, out var drop))
                    {
                        float front = UnityEngine.Random.value;
                        drop.Restart(from, (Vector3.Lerp(-direction, hit.normal + UnityEngine.Random.insideUnitSphere * 0.2f, front)) * (1 + front * front * 3), scale: 0.3f + front * 0.2f);
                    }
                    else
                        break;
                }

                from = hit.point;// + direction*0.8f;

                for (int i = 0; i < bloodParticles; i++)
                {
                    float straight = UnityEngine.Random.value;
                    if (poolOfBlood.TrySpawn(hit.point, out var drop))
                        drop.Restart(from, (direction + UnityEngine.Random.insideUnitSphere * 0.05f) * (1.5f + straight * straight * 6), scale: 0.3f + straight * 0.2f);
                    else
                        break;
                }
            }
        }

        [Serializable]
        public class State : IPEGI
        {
            [SerializeField] public RayfireGun Gun;
            [SerializeField] public C_ParticleSystemBurst MuzzleFlash;
            [NonSerialized] public float WeaponKick = 0;
            
            public readonly LogicWrappers.TimeFixedSegmenter DelayBetweenShots = new(unscaledTime: false, 0.1f, returnOnFirstRequest: 1);

            public override string ToString() => "Machine Gun";

            public void Inspect()
            {
                "Gun Child".PegiLabel(60).Edit(ref Gun).Nl();
                "Muzzle Fkash".PegiLabel(60).Edit(ref MuzzleFlash).Nl();
            }
        }

        #region Inspector

        public override string ToString() => "Machine Gun";

        [SerializeField] private pegi.EnterExitContext context = new();

        public void Inspect()
        {
            using (context.StartContext()) 
            {
                if (context.IsAnyEntered == false)
                {
                    "Range".PegiLabel(50).Edit(ref MaxDistance).Nl();
                }

                "Brush Config".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
                WeaponAttack.Enter_Inspect_AsList().Nl();
            }
        }

        public virtual string NeedAttention()
        {
            if (!brushConfig)
                return "Assign Brush Config";

            var brush = brushConfig.brush;

            if (!brush.Is3DBrush())
                return "Should be a Sphere Brush";

            if (brush.FallbackTarget != TexTarget.RenderTexture)
                return "Should render to Render Texture";

        //    if (brush.GetBlitMode(TexTarget.RenderTexture).GetType() != typeof(BlitModes.Add))
             //   return "Additive is preferred";

            return null;
        }

        #endregion
    }

}
