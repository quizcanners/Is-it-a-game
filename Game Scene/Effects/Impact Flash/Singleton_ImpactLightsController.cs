using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Singleton_ImpactLightsController : PoolSingletonBase<C_ImpactLightForTopDown>
    {
        protected override int MAX_INSTANCES => 50;


        protected override void OnInstanciated(C_ImpactLightForTopDown inst)
        {
            inst.Restart();
        }

    }

    [PEGI_Inspector_Override(typeof(Singleton_ImpactLightsController))] internal class Singleton_ImpactLightsControllerDrawer : PEGI_Inspector_Override { }
}
