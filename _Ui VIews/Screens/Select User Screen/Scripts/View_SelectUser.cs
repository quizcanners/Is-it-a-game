using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class View_SelectUser : UI_TypedView, IPEGI
    {
        public override Game.Enums.View MyView => Game.Enums.View.SelectUser;

        public void Inspect()
        {
            pegi.Nl();
            var users = Game.Persistent.Application.AvailableUsers;

            for(int i=0; i< users.Count; i++) 
            {
                var user = users[i];

                if (Icon.Delete.ClickConfirm(confirmationTag: "DelUsr" + user, toolTip: "Are You sure you want to Delete User Name {0}".F(user)))
                    Game.Persistent.DeleteUser(user);

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
