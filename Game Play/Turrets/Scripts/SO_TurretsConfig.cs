using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop.Turrets
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = Utils.QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class SO_TurretsConfig : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Turrets Config";

        public Weapon_Prototype_Machinegun MachineGun;


        pegi.EnterExitContext _context = new();

        public void Inspect()
        {
            using (_context.StartContext()) 
            {
                MachineGun.Enter_Inspect().Nl();
            }
        }
    }

    [PEGI_Inspector_Override(typeof(SO_TurretsConfig))] internal class SO_TurretsConfigDrawer : PEGI_Inspector_Override { }


}
