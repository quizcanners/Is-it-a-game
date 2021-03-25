namespace QuizCanners.IsItGame.StateMachine
{
    public class MainMenuState : BaseState, IStateDataFallback<IigEnum_Music>, IStateDataFallback<IigEnum_UiView>
    {
        public IigEnum_Music Get() => IigEnum_Music.MainMenu;
        IigEnum_UiView IStateDataFallback<IigEnum_UiView>.Get() => IigEnum_UiView.MainMenu;      
    }
}
