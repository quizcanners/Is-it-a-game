using Mushroom;
using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{

    public class SpaceEffectView : IsItGameOnGuiBehaviourBase, IPEGI
    {
        public override void Inspect()
        {
            Singleton.Try<SpaceAndStarsController>(s=> s.Nested_Inspect(), onFailed: ()=> "Service {0} not found".F(nameof(SpaceAndStarsController)));
        }
    }
}