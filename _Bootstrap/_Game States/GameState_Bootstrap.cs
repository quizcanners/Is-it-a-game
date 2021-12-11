using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class Bootstrap : Base, IPEGI
        {
            internal override void UpdateIfCurrent()
            {
                if (Game.Persistent.User.UserName.IsNullOrEmpty())
                {
                    SetNextState<SelectingUser>();
                }
                else
                {
                    SetNextState<MainMenu>();
                }
            }
        }
    }
}