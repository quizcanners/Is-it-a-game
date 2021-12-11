namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class Gyroscope : Base, IDataFallback<Game.Enums.View>
        {
            public Game.Enums.View Get() => Game.Enums.View.Gyroscope;
        }
    }
}
