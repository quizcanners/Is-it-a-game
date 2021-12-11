using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.SpaceEffect
{
    public class View_SpaceEffect : IsItGameOnGuiBehaviourBase, IPEGI
    {
        public override void Inspect()
        {
            Singleton.Try<Singleton_SpaceAndStars>(s=> s.Nested_Inspect(), onFailed: ()=> "Service {0} not found".F(nameof(Singleton_SpaceAndStars)));
        }
    }
}