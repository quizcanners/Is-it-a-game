using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.UI
{
    public class StateChangingButton : UnityEngine.MonoBehaviour, IPEGI
    {
        [UnityEngine.SerializeField] private bool _exit;
        [UnityEngine.SerializeField] private IigEnum_GameState _targetState;
        public void ChangeState()
        {
            if (_exit)
                _targetState.Exit();
            else
                _targetState.Enter();
        }

        public void Inspect()
        {
            pegi.nl();

            "Exit".PegiLabel().toggleIcon(ref _exit).nl();

            "State to {0}".F(_exit ? "Exit" : "Target").PegiLabel().editEnum(ref _targetState).nl();

            var bttn = GetComponent<UnityEngine.UI.Button>();

            if (bttn && pegi.edit_Listener(bttn.onClick, ChangeState, target:  bttn).nl())
                bttn.SetToDirty();
        }
    }

    [PEGI_Inspector_Override(typeof(StateChangingButton))] internal class StateChangingButtonDrawer : PEGI_Inspector_Override { }

}
