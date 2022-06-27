using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public class Pool_TrailEffectController : PoolSingletonBase<C_TrailingStretch>
    {

        public override void Inspect()
        {
            base.Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_TrailEffectController))] internal class Pool_TrailEffectControllerDrawer : PEGI_Inspector_Override { }
}