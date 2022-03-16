namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class MainMenu : Base, IDataFallback<Game.Enums.Music>, IDataFallback<Game.Enums.View>
        {
            public Game.Enums.Music Get() => Game.Enums.Music.MainMenu;
            Game.Enums.View IDataFallback<Game.Enums.View>.Get() => Game.Enums.View.MainMenu;
        }
    }
}
