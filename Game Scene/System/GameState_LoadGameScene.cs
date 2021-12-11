using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class LoadGameScene : Base, IDataFallback<Scene>, IDataFallback<View>, IDataFallback<UiObscureScreen>
        {
            Scene IDataFallback<Scene>.Get() => Scene.GameScene;
            View IDataFallback<View>.Get() => View.SceneLoading;

            internal override void OnEnter()
            {
                base.OnEnter();

                QcDebug.timerGlobal[nameof(LoadGameScene)].Start();
            }

            internal override void UpdateIfCurrent()
            {
                Singleton.Try<Singleton_Scenes>(s =>
                {
                    if (s.IsLoadedAndInitialized(Scene.GameScene))
                    {
                        SetNextState<PulseCommander>();
                        QcDebug.timerGlobal[nameof(LoadGameScene)].End();
                    }
                });
            }

            public UiObscureScreen Get() => UiObscureScreen.On;
        }
    }
}
