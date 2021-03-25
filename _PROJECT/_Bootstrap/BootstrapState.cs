using QuizCanners.Inspect;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class BootstrapState : BaseState, IPEGI
    {
        internal override void UpdateIfCurrent()
        {
            if (GameEntities.Player.UserName.IsNullOrEmpty()) 
            {
                SetNextState<SelectingUserState>();
            } else 
            {
                SetNextState<MainMenuState>();
            }
        }
    }
}