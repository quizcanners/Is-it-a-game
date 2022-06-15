using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class Bootstrap : Base, IPEGI, IDataFallback<Game.Enums.PhisicalSimulation>
        {
            Game.Enums.PhisicalSimulation IDataFallback<Game.Enums.PhisicalSimulation>.Get() => Game.Enums.PhisicalSimulation.Paused;

            private Gate.Integer _stateVersion = new Gate.Integer();

            internal override void Update()
            {
                base.Update();

                if (_stateVersion.TryChange(Machine.Version))
                {
                    switch (Machine.Get(defaultValue: Game.Enums.PhisicalSimulation.Active))
                    {
                        case Game.Enums.PhisicalSimulation.Active:
                            Time.timeScale = 1;
                            break;
                        case Game.Enums.PhisicalSimulation.Paused:
                            Time.timeScale = 0;
                            break;
                    }
                };
            }

            internal override void UpdateIfCurrent()
            {
                if (Game.Persistent.User.UserName.IsNullOrEmpty())
                {
                    SetNextState<SelectingUser>();
                }
                else
                {
                    SetNextState<MainMenu>();
                }
            }
        }
    }
}