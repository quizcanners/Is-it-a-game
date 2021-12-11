using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;

namespace QuizCanners.IsItGame.Develop
{
    public class View_EditText : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            "Close".PegiLabel().Click().Nl().OnChanged(() => Game.Enums.View.PlayerNameEdit.Hide());

            Game.Persistent.UserInterface.InputFieldData.Nested_Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(View_EditText))]
    internal class View_EditTextDrawer : PEGI_Inspector_Override { }
}
