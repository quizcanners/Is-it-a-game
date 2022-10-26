using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop.Turrets
{
    public class Singleton_TurretsManager : Singleton.BehaniourBase
    {
        [SerializeField] internal SO_TurretsConfig config;

        internal List<C_TurretBase> All_Bases => C_TurretBase.allInstances;
        internal List<Abstract_TurretHead> All_Heads => Abstract_TurretHead.allInstances;

        #region Inspector

        private readonly pegi.EnterExitContext context = new();
        public override void Inspect()
        {
            pegi.Nl();

            using (context.StartContext()) 
            {
                "Config".PegiLabel().Edit_Enter_Inspect(ref config).Nl();

                "Bases".PegiLabel().Enter_List(All_Bases).Nl();
                "Turrets".PegiLabel().Enter_List(All_Heads).Nl();

            }
        }

        public override string ToString() => "Turrets";

        public override string NeedAttention()
        {
            if (!config)
                return "No Config";

            return base.NeedAttention();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_TurretsManager))] internal class Singleton_TurretsManagerDrawer : PEGI_Inspector_Override { }
}
