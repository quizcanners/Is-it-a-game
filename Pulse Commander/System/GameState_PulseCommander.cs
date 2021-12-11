using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class PulseCommander : Base, IDataAdditive<Scene>//, IDataFallback<UiObscureScreen>
        {
            public Scene Get() => Scene.NodeCommander;

            internal override void UpdateIfCurrent()
            {
                Singleton.Try<Singleton_Scenes>(s =>
                {
                    if (s.IsLoadedAndInitialized(Scene.NodeCommander))
                    {
                        SetNextState<CampaignProgressLoaded>();
                        QcDebug.timerGlobal[nameof(LoadGameScene)].End();
                    }
                });
            }

            // 
            // UiObscureScreen IDataFallback<UiObscureScreen>.Get() => UiObscureScreen.On;
        }
    }
}
