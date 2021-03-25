namespace QuizCanners.IsItGame.StateMachine
{
    public class SpaceEffectState : BaseState, IStateDataFallback<IigEnum_Scene>, IStateDataFallback<IigEnum_UiView>
    {
        public IigEnum_Scene Get() => IigEnum_Scene.SpaceEffect;
        IigEnum_UiView IStateDataFallback<IigEnum_UiView>.Get() => IigEnum_UiView.SpaceEffect;
    }
}