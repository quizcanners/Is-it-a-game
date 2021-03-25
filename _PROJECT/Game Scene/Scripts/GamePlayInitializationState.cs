using QuizCanners.IsItGame.StateMachine;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class GamePlayInitializationState : BaseState, IStateDataFallback<IigEnum_Scene>, IStateDataFallback<IigEnum_UiView>
    {
        IigEnum_Scene IStateDataFallback<IigEnum_Scene>.Get() => IigEnum_Scene.GameScene;
        IigEnum_UiView IStateDataFallback<IigEnum_UiView>.Get() => IigEnum_UiView.SceneLoading;

        internal override void OnEnter() 
        {
            base.OnEnter();

            QcDebug.timerGlobal[nameof(GamePlayInitializationState)].Start();
        }

        internal override void UpdateIfCurrent()
        {
            Singleton.Try<ScenesService>(s =>
            {
                if (s.IsLoadedAndInitialized(IigEnum_Scene.GameScene))
                {
                    SetNextState<GamePlayState>();
                    QcDebug.timerGlobal[nameof(GamePlayInitializationState)].End();
                }
            });
        }

        
    }
}
