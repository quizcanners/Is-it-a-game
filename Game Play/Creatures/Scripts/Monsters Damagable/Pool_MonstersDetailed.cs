using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_MonstersDetailed : PoolSingletonBase<C_MonsterDetailed>
    {
        protected override int MAX_INSTANCES => 30;
       

        protected override void OnInstanciated(C_MonsterDetailed inst)
        {
           
        }

        #region Inspector

        public override void InspectInList(ref int edited, int ind)
        {
          

            base.InspectInList(ref edited, ind);
        }

        public override void Inspect()
        {
            if (Application.isPlaying)
            {
               
            }
            base.Inspect();
        }

        #endregion


      
    }

    [PEGI_Inspector_Override(typeof(Pool_MonstersDetailed))] internal class Pool_MonstersControllerDrawer : PEGI_Inspector_Override { }
}
