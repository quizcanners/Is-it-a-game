using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    public class RayFireShootingManager : MonoBehaviour, IPEGI
    {
        [SerializeField] private RayfireGun gun;
        [SerializeField] private RayfireBomb bomb;

        Gate.UnityTimeScaled _shotsGap = new Gate.UnityTimeScaled();

        void Update() 
        {
            if (_testBomb)
            {
                if (Input.GetMouseButton(0) && _shotsGap.TryUpdateIfTimePassed(1f))
                {
                    Ray shootVector = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(shootVector, out var hit))
                    {
                        bomb.gameObject.transform.position = hit.point;
                        bomb.Explode();
                    }
                }

            }
            else
            {

                if (Input.GetMouseButton(0) && _shotsGap.TryUpdateIfTimePassed(0.1f))
                {
                    Ray shootVector = Camera.main.ScreenPointToRay(Input.mousePosition);
                    gun.Shoot(shootVector.origin, shootVector.direction);
                }
            }

        }

        [SerializeField] private bool _testBomb;

        public void Inspect()
        {
            pegi.Nl();
            "Test Bomb".PegiLabel().ToggleIcon(ref _testBomb).Nl();
            "Gun".PegiLabel().Edit_IfNull(ref gun, gameObject).Nl();
            "Bomb".PegiLabel().Edit_IfNull(ref bomb, gameObject).Nl();
        }
    }

    [PEGI_Inspector_Override(typeof(RayFireShootingManager))] internal class RayFireShootingManagerDrawer : PEGI_Inspector_Override { }
}