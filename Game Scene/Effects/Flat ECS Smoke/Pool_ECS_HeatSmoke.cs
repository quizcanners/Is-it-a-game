using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_ECS_HeatSmoke : PoolSingletonBase<C_ECS_HeatSmoke> 
    {
        protected override void OnInstanciated(C_ECS_HeatSmoke inst) 
        {
            inst.Restart();
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_ECS_HeatSmoke))] internal class Pool_ExplosionSmokerDrawer : PEGI_Inspector_Override { }
}