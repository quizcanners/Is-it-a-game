using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Pool_ImpactLightsController : PoolSingletonBase<C_ImpactLightForTopDown>
    {
        protected override void OnInstanciated(C_ImpactLightForTopDown inst)
        {
            inst.SetSize(1);
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_ImpactLightsController))] internal class Singleton_ImpactLightsControllerDrawer : PEGI_Inspector_Override { }
}
