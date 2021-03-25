using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class ViewChangingButton : MonoBehaviour, IPEGI
    {
        [SerializeField] private IigEnum_UiView _targetView;
        [SerializeField] private UiTransitionType _transition;
        [SerializeField] private bool _clearStack;

        public void ChangeView() => _targetView.Show(clearStack: _clearStack, _transition);

        public void Inspect()
        {
            pegi.nl();
            "View".PegiLabel(60).editEnum(ref _targetView).nl();
            "Transition".PegiLabel(80).editEnum(ref _transition).nl();
            "Clear Stack".PegiLabel().toggleIcon(ref _clearStack).nl();
        }
    }

    [PEGI_Inspector_Override(typeof(ViewChangingButton))] internal class ViewChangingButtonDrawer : PEGI_Inspector_Override { }
}
