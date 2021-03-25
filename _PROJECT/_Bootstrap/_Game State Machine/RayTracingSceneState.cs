
namespace QuizCanners.IsItGame.StateMachine 
{
    public class RayTracingSceneState : BaseState, IStateDataFallback<IigEnum_UiView>, IStateDataFallback<IigEnum_Scene>
    {
        public IigEnum_UiView Get() => IigEnum_UiView.RayTracingView;
        IigEnum_Scene IStateDataFallback<IigEnum_Scene>.Get() => IigEnum_Scene.RayTracing;
    }
}
