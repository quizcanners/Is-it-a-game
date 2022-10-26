using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_MonstersGPU_Instanced : PoolSingletonBase<C_MonsterInstanced>// Singleton.BehaniourBase
    {

        protected override int MAX_INSTANCES => 50000;

        #region Inspector

        public override void Inspect()
        {
            base.Inspect();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(Pool_MonstersGPU_Instanced))] internal class Pool_MonstersGPU_InstancedDrawer : PEGI_Inspector_Override { }
}
