using Dungeons_and_Dragons;
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
       
        [SerializeField] private Weapon _weapon;
        private enum Weapon { Machinegun, RocketLauncher, BoltGun }

        [SerializeField] private PlayerGun_Machinegun.State _machineGunState = new();
        [SerializeField] private PlayerGun_RocketLauncher.State _rocketState = new();



        private readonly LogicWrappers.TimeFixedSegmenter _delayBetweenRockets = new( 1.1f, returnOnFirstRequest: 1);

        Singleton_ChornobaivkaController Mgmt => Singleton.Get<Singleton_ChornobaivkaController>();

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
                return Mgmt.CastAll(new Ray(transform.position, hit.point - transform.position), out hit);
            }

            return false;
        }

        private void Update()
        {
            if (GameState.Machine.Get(defaultValue: Game.Enums.PhisicalSimulation.Active) == Game.Enums.PhisicalSimulation.Paused) 
                return;
            
            var isShooting = Input.GetMouseButton(0);

            if (Input.GetKey(KeyCode.Alpha1))
                _weapon = Weapon.Machinegun;

            if (Input.GetKey(KeyCode.Alpha2))
                _weapon = Weapon.RocketLauncher;

            switch (_weapon) 
            {
                case Weapon.BoltGun:

                    break;
                case Weapon.Machinegun: 
                    if (isShooting)
                    {
                        var shots = _machineGunState.DelayBetweenShots.GetSegmentsAndUpdate();

                        if (shots > 0 || Input.GetMouseButtonDown(0))
                        {
                            if (TryHit(out var hit))
                            {
                                _config.MachineGun.Shoot(transform.position, hit.point, _machineGunState);
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
                        if (TryHit(out var hit))
                        {
                            _delayBetweenRockets.GetSegmentsAndUpdate();
                            _config.RocketLauncher.Explosion(hit, hit.point - transform.position, _rocketState);
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
                Gizmos.DrawLine(transform.position, hit.point);
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
                            _rocketState.Nested_Inspect();
                            break;
                    }

                    if (!Camera.main)
                    {
                        "No Main Camera found".PegiLabel().WriteWarning();
                    }
                    else
                    {
                        "Will shoot from {0}".F(Camera.main.gameObject.name).PegiLabel().Write();
                        Camera.main.ClickHighlight().Nl();
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