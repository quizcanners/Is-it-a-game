using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_Monsters_Impostors : PoolSingletonBase<C_Monster_Impostor>
    {
        protected override int MAX_INSTANCES => 50000;

        public override void Inspect()
        {
            base.Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_Monsters_Impostors))]
    internal class Pool_Monsters_ImpostorsDraer : PEGI_Inspector_Override { }
}
