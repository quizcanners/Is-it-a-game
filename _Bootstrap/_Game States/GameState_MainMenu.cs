using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class MainMenu : Base, IDataFallback<IigEnum_Music>, IDataFallback<View>
        {
            public IigEnum_Music Get() => IigEnum_Music.MainMenu;
            View IDataFallback<View>.Get() => View.MainMenu;
        }
    }
}
