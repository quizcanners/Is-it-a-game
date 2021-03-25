using QuizCanners.IsItGame.StateMachine;

namespace QuizCanners.IsItGame.NodeCommander
{
    public class PulseCommanderState : BaseState, IStateDataAdditive<IigEnum_Scene>
    {
        public IigEnum_Scene Get() => IigEnum_Scene.NodeCommander;



    }
}
