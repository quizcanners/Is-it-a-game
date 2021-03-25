using QuizCanners.IsItGame.NodeCommander;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class GamePlayState : BaseState, IStateDataFallback<IigEnum_Music>, IStateDataFallback<IigEnum_UiView>
    {
        public IigEnum_Music Get() => IigEnum_Music.Combat;
        IigEnum_UiView IStateDataFallback<IigEnum_UiView>.Get() => IigEnum_UiView.BackToMainMenuButton;

        internal override void OnEnter() 
        {
            base.OnEnter();
            GameEntities.Player.Campaign.LoadGameToServices();
            _nodeNodesConfigVersion = new Gate.Integer();
        }

        internal override void OnExit()
        {
            GameEntities.Player.Campaign.SaveGameFromServices();
        }

        private Gate.Integer _nodeNodesConfigVersion = new Gate.Integer();

        internal override void UpdateIfCurrent()
        {
            Singleton.Try<ConfigNodesService>(s => 
            {
                if (s.Version == _nodeNodesConfigVersion.CurrentValue) 
                {
                    SetNextState<PulseCommanderState>();
                }
            });
        }

        internal override void Update()
        {
            Singleton.Try<ConfigNodesService>(s => 
            { 
                if (s.AnyEntered && _nodeNodesConfigVersion.TryChange(s.Version)) 
                    s.CurrentChain.LoadConfigsIntoServices();
            });
        }

    }
}
