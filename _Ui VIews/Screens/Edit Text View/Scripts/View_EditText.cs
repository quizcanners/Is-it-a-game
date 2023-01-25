using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;
using TMPro;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class View_EditText : UI_TypedView, IPEGI
    {
        [SerializeField] private TMP_InputField input;

        public override Game.Enums.View MyView => Game.Enums.View.PlayerNameEdit;

        public void Inspect()
        {
            "Close".PegiLabel().Click().Nl().OnChanged(() => Game.Enums.View.PlayerNameEdit.Hide());

            Game.Persistent.UserInterface.InputFieldData.Nested_Inspect();
        }
    }

    [PEGI_Inspector_Override(typeof(View_EditText))]
    internal class View_EditTextDrawer : PEGI_Inspector_Override { }
}
