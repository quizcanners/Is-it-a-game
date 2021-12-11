using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class SpaceEffect : Base, IDataFallback<Scene>, IDataFallback<View>
        {
            public Scene Get() => Scene.SpaceEffect;
            View IDataFallback<View>.Get() => View.SpaceEffect;
        }
    }
}