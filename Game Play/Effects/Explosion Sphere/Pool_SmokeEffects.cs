using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Pool_SmokeEffects : PoolSingletonBase<C_SmokeEffectOnImpact> 
    {
        protected override void OnInstanciated(C_SmokeEffectOnImpact inst)
        {
            inst.Refresh();

            for (int i = 0; i < instances.Count - 1; i++)
            {
                inst.TryConsume(instances[i]);
            }
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_SmokeEffects))] internal class Singleton_SmokeEffectControllerDrawer : PEGI_Inspector_Override { }

}