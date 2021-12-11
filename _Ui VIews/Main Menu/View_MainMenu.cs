using QuizCanners.Inspect;

namespace QuizCanners.IsItGame
{
    public class View_MainMenu : IsItGameBehaviourBase
    {
        public void ChangeUser() 
        {
            Game.Persistent.User.Clear();
            Game.Enums.GameState.Bootstrap.Enter();
        }

        public void Inspect()
        {
            pegi.Click(ChangeUser).Nl();
        }
    }
}