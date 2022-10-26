using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [SelectionBase]
    public class C_PlayerShootingSource : MonoBehaviour, IPEGI, INeedAttention
    {
        
        [SerializeField] private SO_PlayerConfig _config;
        [SerializeField] private GameObject _shootingSource;
        [SerializeField] internal Transform _aimTarget;
        
        [SerializeField] private Weapon _weapon;
        private enum Weapon { Machinegun, RocketLauncher, BoltGun }

        [SerializeField] private Weapon_Prototype_Machinegun.State _machineGunState = new();
        [SerializeField] private PlayerGun_RocketLauncher.State _rocketWeaponState = new();
        [SerializeField] private PlayerGun_BoltGun.State _boltGunState = new();


        private readonly LogicWrappers.TimeFixedSegmenter _delayBetweenRockets = new(unscaledTime: false, 1.1f, returnOnFirstRequest: 1);

        Singleton_ChornobaivkaController Mgmt => Singleton.Get<Singleton_ChornobaivkaController>();

        Vector3 ShootingPosition => _shootingSource ? _shootingSource.transform.position : transform.position;

        private bool TryHit(out RaycastHit hit) 
        {
            var cam = Camera.main;
            if (!cam) 
            {
                hit = new RaycastHit();
                return false;
            }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Mgmt.CastAll(ray, out hit))
            {
                return Mgmt.CastAll(new Ray(ShootingPosition, hit.point - ShootingPosition), out hit);
            }

            return false;
        }

        private readonly Gate.UnityTimeScaled _afterShotSlowAimGate = new();

        private void Update()
        {
            if (GameState.Machine.Get(defaultValue: Game.Enums.PhisicalSimulation.Active) == Game.Enums.PhisicalSimulation.Paused) 
                return;
            
            var isShooting = Input.GetMouseButton(0);

            if (Input.GetKey(KeyCode.Alpha1))
                _weapon = Weapon.Machinegun;

            if (Input.GetKey(KeyCode.Alpha2))
                _weapon = Weapon.RocketLauncher;

            if (Input.GetKey(KeyCode.Alpha3))
                _weapon = Weapon.BoltGun;

            bool isHit = TryHit(out var hit);

            if (isHit) 
            {
                if (_afterShotSlowAimGate.GetDeltaWithoutUpdate() > 0.1f)
                {
                    _aimTarget.transform.position = hit.point;
                } else 
                {
                    _aimTarget.transform.position = Vector3.Lerp(_aimTarget.transform.position, hit.point, Time.deltaTime * 4f);
                }
            }


            switch (_weapon) 
            {
                case Weapon.BoltGun: 
                    if (isShooting) 
                    {
                        if (_boltGunState.DelayBetweenShots.TryUpdateIfTimePassed(0.7f)) 
                        {
                            if (isHit)
                            {
                                _boltGunState.DelayBetweenShots.Update();
                                _config.BoltGun.Shoot(ShootingPosition, hit.point, _boltGunState);
                            }
                        }
                    }
                    
                    break;

                case Weapon.Machinegun: 
                    if (isShooting)
                    {
                        var shots = _machineGunState.DelayBetweenShots.GetSegmentsWithouUpdate();

                        if (shots > 0 || Input.GetMouseButtonDown(0))
                        {
                            if (isHit)
                            {
                                _machineGunState.DelayBetweenShots.Update();
                                _config.MachineGun.Shoot(ShootingPosition, hit.point, _machineGunState, out var actualHit, out var hitMonsters);
                                _aimTarget.transform.position = actualHit;
                                _afterShotSlowAimGate.Update();
                            }
                        }
                    }
                    break;

                case Weapon.RocketLauncher:

                    _config.RocketLauncher.UpdateExplosions();

                if (isShooting)
                {
                    var segm = _delayBetweenRockets.GetSegmentsWithouUpdate();

                    if (segm > 0)
                    {
                        if (isHit)
                        {
                            _delayBetweenRockets.GetSegmentsAndUpdate();
                            _config.RocketLauncher.Explosion(hit, hit.point - ShootingPosition, _rocketWeaponState);
                        }
                    }
                }
                break;
            }

            _machineGunState.WeaponKick = LerpUtils.LerpBySpeed(_machineGunState.WeaponKick, 0, 2f, unscaledTime: false);
        }

        #region Inspector
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && _config && TryHit(out RaycastHit hit))
            {
                var target = hit.transform.GetComponentInParent<C_PaintingReceiver>();
                Gizmos.color = target ? Color.green : Color.blue;
                Gizmos.DrawLine(ShootingPosition, hit.point);
            }
        }

        [SerializeField] private pegi.EnterExitContext context = new();

        public void Inspect()
        {
            pegi.Nl();

            using (context.StartContext())
            {
               
                if (!context.IsAnyEntered)
                {
                    "Wepon".PegiLabel(50).Edit_Enum(ref _weapon).Nl();

                    switch (_weapon) 
                    {
                        case Weapon.Machinegun:
                            _machineGunState.Nested_Inspect();
                            break;
                        case Weapon.RocketLauncher:
                            _rocketWeaponState.Nested_Inspect();
                            break;
                        case Weapon.BoltGun:
                            _boltGunState.Nested_Inspect();
                            break;
                    }

                    if (!Camera.main)
                    {
                        "No Main Camera found".PegiLabel().WriteWarning();
                    }
                    else
                    {
                        "Will shoot from {0}".F(Camera.main.gameObject.name).PegiLabel().Write();
                        pegi.ClickHighlight(Camera.main).Nl();
                    }
                }

                "Config".PegiLabel(60).Edit_Enter_Inspect(ref _config).Nl();
            }
        }

        public virtual string NeedAttention()
        {
            if (!_config)
                return "Config not assigned";

            

            return _config.NeedAttention();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_PlayerShootingSource))]
    internal class C_PlayerShootingSourceDrawer : PEGI_Inspector_Override { }
}