using QuizCanners.Inspect;
using QuizCanners.RayTracing;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Pool_SdfGoreParticles : PoolSingletonBase<C_SdfBloodController>
    {
        public override bool IsSingletonActive()
        {
            if (QcRTX.MOBILE)
                return false;

            return base.IsSingletonActive();
        }

        protected override void OnInstanciated(C_SdfBloodController inst)
        {
            inst.Restart();
        }

    }

    [PEGI_Inspector_Override(typeof(Pool_SdfGoreParticles))] internal class Pool_SdfGoreParticlesDrawer : PEGI_Inspector_Override { }
}
