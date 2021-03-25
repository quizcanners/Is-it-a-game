using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;

namespace QuizCanners.IsItGame.Develop
{
    public class SelectUserView : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            pegi.nl();
            var users = GameEntities.Application.AvailableUsers;

            foreach(var user in users) 
            {
                user.PegiLabel().write();
                "Select".PegiLabel().Click(()=> GameEntities.Player.Load(user)).nl();
            }

            "Create New User".PegiLabel().Click(() =>
            {
                GameEntities.UserInterface.InputFieldData.Set("Create User",
                    onValidate: result => users.Add(result),
                    // onClose: () => UiViewType.SelectUser.Show(),
                    validator: text => text != null && text.Length > 3 && users.Contains(text) == false
                    ); ;
                IigEnum_UiView.PlayerNameEdit.Show(clearStack: false);
            });
        }
    }

    [PEGI_Inspector_Override(typeof(SelectUserView))] internal class SelectUserViewDrawer : PEGI_Inspector_Override { }
}
