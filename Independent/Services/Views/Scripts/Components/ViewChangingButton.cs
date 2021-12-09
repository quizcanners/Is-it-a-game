using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class ViewChangingButton : MonoBehaviour, IPEGI
    {
        [SerializeField] private bool _closeCurrent;
        [SerializeField] private IigEnum_UiView _targetView;
        [SerializeField] private UiTransitionType _transition;
        [SerializeField] private bool _clearStack;

        public void ChangeView()
        {
            if (_closeCurrent)
                Singleton.Try<UiViewService>(s => s.HideCurrent(_transition));
            else 
                _targetView.Show(clearStack: _clearStack, _transition);
        }
        public void Inspect()
        {

            pegi.nl();

            "Close Current".PegiLabel().toggleIcon(ref _closeCurrent).nl();
            "Transition".PegiLabel(80).editEnum(ref _transition).nl();

            if (!_closeCurrent)
            {
                "View".PegiLabel(60).editEnum(ref _targetView).nl();
                "Clear Stack".PegiLabel().toggleIcon(ref _clearStack).nl();
            }

            var bttn = GetComponent<UnityEngine.UI.Button>();

            if (bttn && pegi.edit_Listener(bttn.onClick, ChangeView, target: bttn).nl())
                bttn.SetToDirty();

        }
    }

    [PEGI_Inspector_Override(typeof(ViewChangingButton))] internal class ViewChangingButtonDrawer : PEGI_Inspector_Override { }
}
