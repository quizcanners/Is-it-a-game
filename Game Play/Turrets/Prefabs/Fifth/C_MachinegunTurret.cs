using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuizCanners.IsItGame.Develop.Turrets
{
    public class C_MachinegunTurret : Abstract_TurretHead, IPEGI
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
        private readonly Gate.UnityTimeUnScaled _targetSerchDelay = new();
        private readonly List<C_Monster_Data.InstanceReference> _trackedMonsters= new();
        private int _missTargetCount;
        private int _lastCheckedTarget;

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
                                _targetMonster = _trackedMonsters.TryTake(i);
                                break;
                            }
                        }  
                    }

                    //_lastCheckedTarget

                    Singleton.Try<Pool_Monsters_Data>(s =>
                    {
                        if (s.TryIterate(ref _lastCheckedTarget, out var newTarget))
                        {
                            _targetMonster = newTarget.GetReference();
                        }
                    });

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
                    _missTargetCount = 0;
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
                    _machineGunState.DelayBetweenShots.Update();
                    Mgmt.config.MachineGun.Shoot(gunBarrel.position, gunBarrel.position + gun.forward, _machineGunState, out var actualHit, out List<C_Monster_Data> mosnters);

                    if (mosnters.Count == 0)
                    {
                        _missTargetCount++;
                        if (_missTargetCount > 5)
                            _targetMonster = new();
                    }
                    else
                    {
                        _missTargetCount = Mathf.Max(0, _missTargetCount - 1);
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
        }


        #region Inspector

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

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_MachinegunTurret))] internal class C_MachinegunTurretDrawer : PEGI_Inspector_Override { }
}