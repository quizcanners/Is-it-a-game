using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;

namespace QuizCanners.IsItGame.Develop
{
    public class View_SelectUser : IsItGameOnGuiBehaviourBase, IPEGI
    {
        public override void Inspect()
        {
            pegi.Nl();
            var users = Game.Persistent.Application.AvailableUsers;

            foreach(var user in users) 
            {
                user.PegiLabel().Write();
                "Select".PegiLabel().Click(()=> Game.Persistent.User.Load(user)).Nl();
            }

            "Create New User".PegiLabel().Click(() =>
            {
                Game.Persistent.UserInterface.InputFieldData.Set("Create User",
                    onValidate: result => users.Add(result),
                    // onClose: () => UiViewType.SelectUser.Show(),
                    validator: text => text != null && text.Length > 3 && users.Contains(text) == false
                    ); ;
                Game.Enums.View.PlayerNameEdit.Show(clearStack: false);
            });
        }
    }

    [PEGI_Inspector_Override(typeof(View_SelectUser))] internal class SelectUserViewDrawer : PEGI_Inspector_Override { }
}
