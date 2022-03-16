using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class CampaignProgressLoaded : Base, IDataFallback<Game.Enums.Music>, IDataFallback<Game.Enums.View>, IDataFallback<UiObscureScreen>
        {
            public Game.Enums.Music Get() => Game.Enums.Music.Combat;

            UiObscureScreen IDataFallback<UiObscureScreen>.Get() => UiObscureScreen.Off;

            Game.Enums.View IDataFallback<Game.Enums.View>.Get() => Game.Enums.View.BackToMainMenuButton;

            internal override void OnEnter()
            {
                base.OnEnter();
                Game.Persistent.User.Campaign.LoadGameToServices();
                _nodeNodesConfigVersion = new Gate.Integer();
            }

            internal override void OnExit()
            {
                Game.Persistent.User.Campaign.SaveGameFromServices();
            }

            private Gate.Integer _nodeNodesConfigVersion = new();

            internal override void UpdateIfCurrent()
            {
                /*Singleton.Try<Singleton_ConfigNodes>(s =>
                {
                    if (s.Version == _nodeNodesConfigVersion.CurrentValue)
                    {
                        SetNextState<PulseCommander>();
                    }
                });*/
            }

            internal override void Update()
            {
                Singleton.Try<Singleton_ConfigNodes>(s =>
                {
                    if (s.AnyEntered && _nodeNodesConfigVersion.TryChange(s.Version))
                        s.CurrentChain.LoadConfigsIntoServices();
                });
            }
        }
    }
}
