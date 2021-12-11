
namespace QuizCanners.IsItGame.StateMachine 
{
    partial class GameState
    {
        public class RayTracingScene : Base, IDataFallback<Game.Enums.View>, IDataFallback<Game.Enums.Scene>
        {
            public Game.Enums.View Get() => Game.Enums.View.RayTracingView;
            Game.Enums.Scene IDataFallback<Game.Enums.Scene>.Get() => Game.Enums.Scene.RayTracing;
        }
    }
}
