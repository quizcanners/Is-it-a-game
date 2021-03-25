using QuizCanners.IsItGame.StateMachine;


namespace QuizCanners.IsItGame.Develop
{
    public class GyroscopeState : BaseState, IStateDataFallback<IigEnum_UiView>
    {
        public IigEnum_UiView Get() => IigEnum_UiView.Gyroscope;
    }
}
