using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Pool_SdfGoreParticles : PoolSingletonBase<C_SdfBloodController>
    {
        protected override void OnInstanciated(C_SdfBloodController inst)
        {
            inst.Restart();
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_SdfGoreParticles))] internal class Pool_SdfGoreParticlesDrawer : PEGI_Inspector_Override { }
}
