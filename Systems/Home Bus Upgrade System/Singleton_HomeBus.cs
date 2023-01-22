using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.HomeBase
{
    [ExecuteAlways]
    public class Singleton_HomeBus : Singleton.BehaniourBase
    {



        #region Inpector

        public override void Inspect()
        {
            base.Inspect();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_HomeBus))] internal class Singleton_HomeBusDrawer : PEGI_Inspector_Override { }
}
