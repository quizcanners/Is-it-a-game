using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;
using static QuizCanners.IsItGame.Game.Enums;

namespace QuizCanners.IsItGame.StateMachine
{
    partial class GameState
    {
        public class Bootstrap : Base, IPEGI, IDataFallback<Scene>, IDataFallback<PhisicalSimulation>
        {
            PhisicalSimulation IDataFallback<PhisicalSimulation>.Get() => PhisicalSimulation.Paused;
            public Scene Get() => Scene.MainMenu;

            private readonly Gate.Integer _stateVersion = new();

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
                    Singleton.Try<Singleton_Scenes>(s =>
                    {
                        if (s.IsLoadedAndInitialized(Scene.MainMenu))
                        {
                            SetNextState<MainMenu>();
                        }
                    });
                }
            }
        }
    }
}