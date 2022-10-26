using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [DisallowMultipleComponent]
    public class Pool_BoltProjectiles : PoolSingletonBase<C_BoltProjectile>
    {
        


    }

    [PEGI_Inspector_Override(typeof(Pool_BoltProjectiles))] internal class Pool_BoltProjectilesDrawer : PEGI_Inspector_Override { }
}