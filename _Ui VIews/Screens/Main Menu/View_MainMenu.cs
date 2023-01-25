using QuizCanners.Inspect;

namespace QuizCanners.IsItGame
{
    public class View_MainMenu : UI_TypedView
    {
        public override Game.Enums.View MyView => Game.Enums.View.MainMenu;

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