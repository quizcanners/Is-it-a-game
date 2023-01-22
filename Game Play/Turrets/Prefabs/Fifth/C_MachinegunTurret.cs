using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
using static QuizCanners.IsItGame.Develop.C_Monster_Data;

namespace QuizCanners.IsItGame.Develop.Turrets
{

    [SelectionBase]
    public class C_MachinegunTurret : Abstract_TurretHead, IPEGI, IPEGI_Handles
    {
        [SerializeField] private Weapon_Prototype_Machinegun.State _machineGunState = new();

        [SerializeField] private Transform rotationBase;
        [SerializeField] private Transform gun;
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private float _rotationSpeed = 10;
        [SerializeField] private float _max_rotationSpeed = 200;

        private C_Monster_Data.InstanceReference _targetMonster;
        private bool _debugMode;
        private float _activeSpeed;
        private Quaternion targetRotation;
        //private readonly Gate.UnityTimeUnScaled _targetSerchDelay = new();
        private readonly Gate.UnityTimeScaled _sinceLastShotAtEnemy = new Gate.UnityTimeScaled();
        private readonly List<C_Monster_Data.InstanceReference> _trackedMonsters= new();
        private int _missTargetCount;
        private int _lastCheckedTarget;
        private bool _anyEnemies;

        

        Singleton_TurretsManager Mgmt => Singleton.Get<Singleton_TurretsManager>();



        // Update is called once per frame
        void Update()
        {
            float dist = Quaternion.Angle(gun.rotation, targetRotation);

            if (dist > 0.0001f)
            {
                _activeSpeed = Mathf.Lerp(_activeSpeed, Mathf.Min(_max_rotationSpeed, dist * 10 + 1), _rotationSpeed * Time.deltaTime);

                var rot = LerpUtils.LerpBySpeed(gun.rotation, targetRotation, _activeSpeed, unscaledTime: false);

                var eul = rot.eulerAngles;
                rotationBase.rotation = Quaternion.Euler(new Vector3(0, eul.y, 0));
                gun.rotation = rot;
            } else
            {
                _activeSpeed = 0;
            }

            if (!_debugMode)
            {
                if (_targetMonster.IsValid) 
                {
                    var toTarget = _targetMonster.enemy.GetActivePosition() - gun.position;
                    targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);
                    TryShoot();

                    if (_missTargetCount > 0) 
                    {
                        TryGetNewEnemy();
                    }

                }
                else
                {
                    _targetMonster.Clear();

                    if (_trackedMonsters.Count > 0)
                    {
                        for (int i = _trackedMonsters.Count - 1; i >= 0; i--)
                        {
                            var m = _trackedMonsters[i];
                            if (!m.IsValid)
                            {
                                _trackedMonsters.RemoveAt(i);
                            }
                            else
                            {
                                SetNewTarget(_trackedMonsters.TryTake(i));
                                _missTargetCount = 0;
                                break;
                            }
                        }  
                    }

                    //_lastCheckedTarget
                    TryGetNewEnemy();


                
                    /*if (_targetSerchDelay.TryUpdateIfTimePassed(0.4f))
                    {
                        Singleton.Try<Pool_Monsters_Data>(s =>
                        {
                            if (s.TryGetNearest(transform.position, out var nearestOnr))
                            {
                                _targetMonster = nearestOnr.GetReference();
                            }
                        }
                        );
                    }*/
                  

                    if (_anyEnemies && _sinceLastShotAtEnemy.GetDeltaWithoutUpdate() < 1) // Shooting even if there are no enemies in sight
                        TryShoot();
                }
            }
            else 
            { 
                if (Input.GetMouseButton(0))
                {
                    Singleton.Try<Singleton_ChornobaivkaController>(s =>
                    {
                        if (s.CastAll(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
                        {
                            var toTarget = hit.point - gun.position;
                            targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);

                        }

                        TryShoot();
                    });
                }
            }



            bool TryShoot() 
            {
                var shots = _machineGunState.DelayBetweenShots.GetSegmentsWithouUpdate();

                if (shots > 0)
                {
                    if (_targetMonster.IsValid)
                        _sinceLastShotAtEnemy.Update();

                    _machineGunState.DelayBetweenShots.Update();
                    Mgmt.config.MachineGun.Shoot(gunBarrel.position, gunBarrel.position + gun.forward, _machineGunState, out var actualHit, out List<C_Monster_Data> mosnters);

                    if (mosnters.Count == 0)
                    {
                        bool FinishedRotation() => dist < 0.01;

                        if (FinishedRotation())
                        {
                            _missTargetCount++;
                            if (_missTargetCount > 10)
                                _targetMonster = new();
                        }
                    }
                    else
                    {
                        _anyEnemies = true;
                        _missTargetCount = Mathf.Max(0, _missTargetCount - mosnters.Count);
                        foreach (var m in mosnters)
                        {
                            var reff = m.GetReference();
                            _trackedMonsters.AddIfNew(reff);
                        }
                    }
                   
                    Pool.TrySpawnIfVisible<C_ImpactLightForTopDown>(gunBarrel.transform.position.Y(0),
                        instance =>
                        {
                            instance.SetSize(20);
                        });

                    return true;
                }

                return false;
            }


            void TryGetNewEnemy()
            {
                Singleton.Try<Pool_Monsters_Data>(s =>
                {
                    if (s.TryIterate(ref _lastCheckedTarget, out var newTarget))
                    {
                        Singleton.Try<Singleton_ChornobaivkaController>(ch =>
                        {
                            var from = gunBarrel.position;
                            var to = newTarget.GetActivePosition();

                            _debug_lastHitCheck = to;

                            if (ch.CastHardSurface(new Ray(from, to - from), out var hit))
                            {
                                if (Vector3.Distance(hit.point, to) < 2)
                                {
                                    SetNewTarget(newTarget.GetReference());
                                }
                                else
                                {
                                    CreatureProxy_Base proxy = hit.transform.gameObject.GetComponentInParent<CreatureProxy_Base>();

                                    if (proxy && proxy.IsAlive)
                                    {
                                        _lastHitCheckState = true;

                                        if (_targetMonster.IsValid && _missTargetCount < 3)
                                        {
                                            float currentDist = (_targetMonster.enemy.GetActivePosition() - from).sqrMagnitude;
                                            float newDist = (proxy.Parent.GetActivePosition() - from).sqrMagnitude;
                                            if (newDist < currentDist)
                                            {
                                                SetNewTarget(proxy.Parent.GetReference());
                                            }
                                        }
                                        else
                                        {
                                            SetNewTarget(proxy.Parent.GetReference());
                                        }
                                    }
                                    else
                                    {
                                        _lastHitCheckState = false;
                                    }
                                }
                            }

                        }, onFailed: () =>
                        {
                            if (_targetMonster.IsValid == false)
                                SetNewTarget(newTarget.GetReference());
                        });
                    }
                }, logOnServiceMissing: false);

            }

            void SetNewTarget(InstanceReference data) 
            {
                _targetMonster = data;
                _missTargetCount = 0;
            }

        }


        #region Inspector

        private Vector3 _debug_lastHitCheck;
        private bool _lastHitCheckState;

        private readonly pegi.EnterExitContext _context = new();

        public void Inspect()
        {
            if (Application.isPlaying == false)
            {
                pegi.TryDefaultInspect(this);
            }

            using (_context.StartContext()) 
            {
                if (_context.IsAnyEntered == false) 
                {
                    "Debug Mode".PegiLabel().ToggleIcon(ref _debugMode).Nl();

                    if (!_debugMode)
                        pegi.Nested_Inspect(ref _targetMonster).Nl();
                }

                _machineGunState.Enter_Inspect().Nl();

                

            }
        }

        public void OnSceneDraw()
        {
            pegi.Handle.Line(gunBarrel.transform.position, _debug_lastHitCheck, _lastHitCheckState ? Color.green : Color.red);
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_MachinegunTurret))] internal class C_MachinegunTurretDrawer : PEGI_Inspector_Override { }
}