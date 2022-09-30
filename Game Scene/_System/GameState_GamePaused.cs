using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class Paused : Base, 
            IDataFallback<Game.Enums.PhisicalSimulation>, 
            IDataFallback<Game.Enums.View>,
            IDataFallback<Game.Enums.Music>
        {
            public UiObscureScreen Get() => UiObscureScreen.On;

            Game.Enums.PhisicalSimulation IDataFallback<Game.Enums.PhisicalSimulation>.Get() => Game.Enums.PhisicalSimulation.Paused;

            Game.Enums.View IDataFallback<Game.Enums.View>.Get() => Game.Enums.View.PauseMenu;
            Game.Enums.Music IDataFallback<Game.Enums.Music>.Get() => Game.Enums.Music.PauseMenu;
        }
    }
}
