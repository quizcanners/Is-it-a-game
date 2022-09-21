using QuizCanners.IsItGame.Develop;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using UnityEngine.UIElements;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class CampaignProgressLoaded : Base, 
            IDataFallback<Game.Enums.Music>, 
            IDataFallback<Game.Enums.View>, 
            IDataFallback<UiObscureScreen>,
            IDataFallback<Game.Enums.PhisicalSimulation>
        {

            bool GotMonsters => Singleton.TryGetValue<Pool_MonstersController, bool>(s => s.Autospawn, defaultValue: false, logOnServiceMissing: false);

            public Game.Enums.Music Get() => GotMonsters ? Game.Enums.Music.Combat : Game.Enums.Music.Exploration;
            
            Game.Enums.PhisicalSimulation IDataFallback<Game.Enums.PhisicalSimulation>.Get() => Game.Enums.PhisicalSimulation.Active;

            UiObscureScreen IDataFallback<UiObscureScreen>.Get() => UiObscureScreen.Off;

            Game.Enums.View IDataFallback<Game.Enums.View>.Get() => Game.Enums.View.BackToMainMenuButton;

            internal override void OnEnter()
            {
                base.OnEnter();
                Game.Persistent.User.Campaign.LoadGameToServices();
                _nodeNodesConfigVersion = new Gate.Integer();

                Singleton.Try<Singleton_ConfigNodes>(s => s.SetDefaultNode());
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

            private Gate.Bool _monstersState = new Gate.Bool();

            internal override void Update()
            {
                if (_monstersState.TryChange(GotMonsters))
                    Machine.SetDirty();

                Singleton.Try<Singleton_ConfigNodes>(s =>
                {
                    if (s.AnyEntered)
                    {
                        if (_nodeNodesConfigVersion.TryChange(s.Version))
                            s.CurrentChain.LoadConfigsIntoServices();
                    } 
                });
            }
        }
    }
}
