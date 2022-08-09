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
    public class PlayerGun_Machinegun :  IPEGI, INeedAttention
    {
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;
      
        public Attack WeaponAttack = new(name: "Gun", isRange: true, attackBonus: 3,
             new Damage()
             {
                 DamageBonus = 2,
                 DamageDice = new List<Dice> { Dice.D10 },
                 DamageType = DamageType.Piercing
             });

        public float MaxDistance = 2000;

        public void Shoot(Vector3 from, Vector3 target, State state)
        {
            Game.Enums.SoundEffects.Shot.PlayOneShotAt(from, clipVolume: 3);

            var spread = UnityEngine.Random.insideUnitSphere * state.WeaponKick;

            var mgmt = Singleton.Get<Singleton_ChornobaivkaController>();
            if (!mgmt)
            {
                QcLog.ChillLogger.LogErrorOnce("{0} not found".F(nameof(Singleton_ChornobaivkaController)), key: "NoChrnb");
                return;
            }

            var ray = new Ray(from + spread, target - from);

            RaycastHit firstHit;

            if (mgmt.CastHardSurface(ray, out firstHit) || mgmt.CastPierce(ray, out firstHit))
            {
                Vector3 direction = (firstHit.point - from).normalized;

                Singleton.Try<Pool_TrailEffectController>(s =>
                {
                    s.TrySpawn(from, out var trace);
                    trace.FlyTo(firstHit.point);
                });

                Singleton.Try<Singleton_CameraOperatorGodMode>(c =>
                {
                    var point = QcMath.GetClosestPointOnALine(lineA: from, lineB: firstHit.point, point: c.transform.position);
                    Game.Enums.SoundEffects.BulletFlyBy.PlayOneShotAt(point, clipVolume: 0.5f);
                });

                state.WeaponKick = Mathf.Clamp01(state.WeaponKick + 0.3f);

                ProcessHit(firstHit, ray.origin, direction, out bool pierced, state);

                int maxLine = 10;

                RaycastHit latestHit = firstHit;

                while (pierced && maxLine > 0) 
                {
                    maxLine--;
                    pierced = false;

                    var outRay = new Ray(origin: latestHit.point, direction: direction);

                    if (mgmt.CastHardSurface(outRay, out latestHit)) 
                    {
                        ProcessHit(latestHit, ray.origin, direction, out pierced, state);
                    }
                }

                if (mgmt.CastPierce(ray, firstHit, out var softHit)) 
                {
                    ProcessHit(softHit, ray.origin, direction, out pierced, state);
                }

            }
        }

        private void ProcessHit(RaycastHit hit, Vector3 origin, Vector3 direction, out bool pierced, State state) 
        {
            bool visibleByCamera = Camera.main.IsInCameraViewArea(hit.point);

            pierced = false;

            if (visibleByCamera)
            {
                bool isSkinned = false;

                var receivers = hit.transform.GetComponentsInParent<C_PaintingReceiver>();

                if (receivers.Length > 0)
                {
                    C_PaintingReceiver receiver = receivers.GetByHit(hit, out int subMesh);

                    if (receiver)
                    {
                        ApplyBrush(out isSkinned);

                        void ApplyBrush(out bool isSkinned)
                        {
                            var tex = receiver.GetTexture();

                            isSkinned = false;

                            if (tex)
                            {
                                if (tex is Texture2D d)
                                {
                                    if (hit.collider.GetType() != typeof(MeshCollider))
                                        Debug.Log("Can't get UV coordinates from a Non-Mesh Collider");

                                    BlitFunctions.Paint(receiver.useTexcoord2 ? hit.textureCoord2 : hit.textureCoord, 1, d, Vector2.zero, Vector2.one, brushConfig.brush);
                                    var id = tex.GetTextureMeta();

                                    return;
                                }

                                var rendTex = tex as RenderTexture;

                                if (rendTex)
                                {
                                    float wallShootTrough = 0.5f;

                                    var hitVector = direction;//(hit.point - from).normalized;

                                    var st = receiver.CreateStroke(hit, hitVector.normalized * (wallShootTrough + 0.2f));

                                    st.posFrom -= hitVector * 0.2f;

                                    isSkinned = receiver.type == C_PaintingReceiver.RendererType.Skinned;

                                    if ((isSkinned && receiver.skinnedMeshRenderer)
                                        || (receiver.type == C_PaintingReceiver.RendererType.Regular && receiver.meshFilter))
                                    {
                                        BrushTypes.Sphere.Paint(receiver.CreatePaintCommandForSphereBrush(st, brushConfig.brush, subMesh));
                                    }
                                    else
                                    {
                                        QcLog.ChillLogger.LogErrorOnce("wasn't setup right for painting", "NoRtPntng");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var monster = hit.transform.gameObject.GetComponentInParent<C_MonsterEnemy>();

            if (monster)
            {

                if (visibleByCamera && monster.ShowDamage &&
                  (monster.LimbsState == C_MonsterEnemy.LimbsControllerState.Animation || monster.LimbsState == C_MonsterEnemy.LimbsControllerState.Ragdoll))
                {
                    monster.ImpactController.Play(hit.point, 1, disintegrate: false, origin: hit.transform);
                }


                if (!monster.IsAlive && monster.LimbsState == C_MonsterEnemy.LimbsControllerState.Animation)
                {
                    monster.DropRigid();
                    pierced = true;
                }

              
                if (monster.TryTakeHit(WeaponAttack, RollInfluence.Advantage, C_MonsterEnemy.LimbsControllerState.Animation))
                {
                    if (visibleByCamera)
                        SpawnBlood(hit, direction);

                    if (monster.ImpactController && monster.IsAlive)
                    {
                        if (visibleByCamera)
                        {
                            monster.ShowDamage = true;
                        }

                        Game.Enums.SoundEffects.BodyImpact.PlayOneShotAt(monster.transform.position, clipVolume: 2);
                    }
                    else
                        if (visibleByCamera)
                        Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawnIfVisible(hit.point, onInstanciate: l => l.SetSize(10f)));

                    if (visibleByCamera)
                    {
                        switch (monster.LimbsState)
                        {
                            case C_MonsterEnemy.LimbsControllerState.Ragdoll:
                                if (monster.TryGetNearestRigidbody(hit.point, out var rb, out float dist))
                                {
                                    rb.AddForce((Vector3.up + UnityEngine.Random.insideUnitSphere) * 25, mode: ForceMode.Impulse);
                                    rb.AddTorque(UnityEngine.Random.insideUnitSphere * 15, ForceMode.Impulse);
                                    // rb.AddForce(direction * 50, mode: ForceMode.Impulse);

                                    monster.postDeathDamage += 1;

                                    if (monster.postDeathDamage > 3)
                                        monster.Giblets();
                                }
                                break;

                        }
                    }

                    pierced = !monster.IsAlive;
                }
                else
                {
                    Game.Enums.SoundEffects.ArmorImpact.PlayOneShotAt(monster.transform.position);

                    if (visibleByCamera)
                        Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawnIfVisible(hit.point, onInstanciate: l => l.SetSize(10f)));
                }
            }
            else
            {

                Game.Enums.SoundEffects.DefaultSurfaceImpact.PlayOneShotAt(hit.point);


                if (!visibleByCamera)
                    return;

                Singleton.Try<Pool_ImpactLightsController>(s => s.TrySpawn(hit.point, onInstanciate: l => l.SetSize(10f)));

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

                //Singleton.Try<Pool_ECS_HeatSmoke>(s => s.TrySpawn(worldPosition: hit.point));

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

                            debri.Size = (0.02f + big * 0.5f);

                            var dir = Vector3.Lerp(reflected, hit.normal, big);

                            debri.Push(pushVector: dir, pushForce: debri.Size * 12f, pushRandomness: 0.1f, torqueForce: 0);
                        }
                        else break;
                    }
                });
            }
        }

        private void SpawnBlood(RaycastHit hit, Vector3 direction)
        {
            var poolOfBlood = Singleton.Get<Pool_BloodParticlesController>();

            if (poolOfBlood)
            {

                int bloodParticles = Mathf.RoundToInt(Mathf.Max(1, poolOfBlood.VacancyPortion * 5));

                Vector3 from = hit.point;// + direction * 0.2f;

                for (int i = 0; i < bloodParticles; i++)
                {
                    if (poolOfBlood.TrySpawn(hit.point, out var drop))
                    {
                        float front = UnityEngine.Random.value;
                        drop.Restart(from, (Vector3.Lerp(-direction, hit.normal + UnityEngine.Random.insideUnitSphere * 0.2f, front)) * (1 + front * front * 3), scale: 1f + front);
                    }
                    else
                        break;
                }

                from = hit.point;// + direction*0.8f;

                for (int i = 0; i < bloodParticles; i++)
                {
                    float straight = UnityEngine.Random.value;
                    if (poolOfBlood.TrySpawn(hit.point, out var drop))
                        drop.Restart(from, (direction + UnityEngine.Random.insideUnitSphere * 0.05f) * (1.5f + straight * straight * 6), scale: 1f + straight);
                    else
                        break;
                }
            }
        }

        [Serializable]
        public class State : IPEGI
        {
            [SerializeField] public RayfireGun Gun;
            [NonSerialized] public float WeaponKick = 0;
            

            public readonly LogicWrappers.TimeFixedSegmenter DelayBetweenShots = new(0.1f, returnOnFirstRequest: 1);

            public void Inspect()
            {
                "Gun Child".PegiLabel(60).Edit(ref Gun).Nl();
            }
        }

        #region Inspector

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
