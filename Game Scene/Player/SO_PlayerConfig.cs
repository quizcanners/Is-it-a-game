using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = Utils.QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/Config/" + FILE_NAME)]
    public class SO_PlayerConfig : ScriptableObject, IPEGI, INeedAttention
    {
        public const string FILE_NAME = "Player Config";

        public PlayerGun_RocketLauncher RocketLauncher;
        public PlayerGun_Machinegun MachineGun;
        public PlayerGun_BoltGun BoltGun;

        [SerializeField] private pegi.EnterExitContext context = new();

        public void Inspect()
        {
            pegi.Nl();

            using (context.StartContext()) 
            {
                RocketLauncher.Enter_Inspect().Nl();
                MachineGun.Enter_Inspect().Nl();
                BoltGun.Enter_Inspect().Nl();
            }
        }

        public string NeedAttention()
        {
            if (MachineGun.TryGetAttentionMessage(out var msg))
                return msg;

            if (BoltGun.TryGetAttentionMessage(out msg))
                return msg;

            return null;
        }
    }

    [PEGI_Inspector_Override(typeof(SO_PlayerConfig))] internal class SO_PlayerConfigDrawer : PEGI_Inspector_Override { }
}