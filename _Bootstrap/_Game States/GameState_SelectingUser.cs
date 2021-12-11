using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class SelectingUser : Base, IDataFallback<Game.Enums.View>
        {
            public Game.Enums.View Get() => Game.Enums.View.SelectUser;

            internal override void Update()
            {
                if (Game.Persistent.User.UserName.IsNullOrEmpty() == false)
                {
                    Exit();
                }
            }
        }
    }
}
