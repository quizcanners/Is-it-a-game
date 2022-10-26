using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class LoadGameScene : Base, IDataFallback<Scene>, IDataAdditive<Scene> , IDataFallback<View>, IDataFallback<UiObscureScreen>
        {
            Scene IDataFallback<Scene>.Get() => Scene.GameScene;
            Scene IDataAdditive<Scene>.Get() => Scene.Terrain;

            View IDataFallback<View>.Get() => View.SceneLoading;

            System.IDisposable timer;

            internal override void OnEnter()
            {
                base.OnEnter();
                timer = QcDebug.TimeProfiler.Instance.Max(nameof(LoadGameScene)).Start();
            }

            internal override void UpdateIfCurrent()
            {
                Singleton.Try<Singleton_Scenes>(s =>
                {
                    if (s.IsLoadedAndInitialized(Scene.GameScene))
                    {
                        SetNextState<CampaignProgressLoaded>();
                        timer.Dispose();
                    }
                });
            }

            public UiObscureScreen Get() => UiObscureScreen.On;

           
        }
    }
}
