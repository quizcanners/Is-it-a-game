
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{


    public abstract class IsItGameOnGuiBehaviourBase : IsItGameBehaviourBase, IPEGI
    {
        public abstract void Inspect();

        private readonly pegi.GameView.Window _window = new pegi.GameView.Window(upscale: 2);

        private void OnGUI()
        {
            Singleton.Try<InspectorOnGuiService>(s =>
            {
                if (!s.DrawInspector)
                {
                    _window.Render(this);
                }
            });
        }
    }

    public class IsItGameBehaviourBase : MonoBehaviour
    {
        protected GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected PersistentGameStateData GameEntities => Mgmt.PersistentProgressData;
        protected StateMachine.Manager GameStateMachine => Mgmt.StateMachine;
    }

    public class IsItGameServiceBase : Singleton.BehaniourBase, IGotVersion
    {
        protected GameController Mgmt => GameController.instance;
        protected StateMachine.Manager GameStateMachine => Mgmt.StateMachine;


        public int Version { get; private set; }
        protected void SetDirty() => Version++;

        protected Gate.Integer _checkedStateVersion = new Gate.Integer();
        protected bool TryEnterIfStateChanged() => Application.isPlaying && _checkedStateVersion.TryChange(GameStateMachine.Version);

        public override void Inspect()
        {
            base.Inspect();

            pegi.nl();

            "Checked Version: {0}".F(_checkedStateVersion.CurrentValue).PegiLabel().write();

            if (icon.Refresh.Click())
            {
                _checkedStateVersion.TryChange(-1);
            }

            pegi.nl();

        }

    }
}
