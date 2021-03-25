using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;

namespace QuizCanners.IsItGame.Develop
{
    public class EditTextView : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            "Close".PegiLabel().Click().nl().OnChanged(() => IigEnum_UiView.PlayerNameEdit.Hide());

            GameEntities.UserInterface.InputFieldData.Nested_Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(EditTextView))]
    internal class EditTextViewDrawer : PEGI_Inspector_Override { }
}
