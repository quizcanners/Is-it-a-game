using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame
{
    public partial class SO_PersistentGameData : IPEGI
    {
        public Persistent_UserData UserInterface = new Persistent_UserData(); // Ui
        public Persistent_ApplicationData Application = new Persistent_ApplicationData(); // All users
        public Persistent_PlayerData User = new Persistent_PlayerData();  // Specific user

        #region Inspector
        private int _inspectedStuff = -1;
        public void Inspect()
        {
            if (_inspectedStuff == -1)
            {
                string tmp = User.UserName;
                if (Icon.Delete.ClickConfirm(confirmationTag: "delUsr", toolTip: "Delete this User?"))
                {
                    Application.AvailableUsers.Remove(tmp);
                    User.Delete();
                }

                if ("User".PegiLabel(60).Select(ref tmp, Application.AvailableUsers))
                {
                    User.Save();
                    User.Load(tmp);
                }

                Icon.Folder.Click(() => QcFile.Explorer.OpenPersistentFolder()).Nl();
            }

            int section = -1;

            "Player".PegiLabel().Enter_Inspect(User, ref _inspectedStuff, ++section).Nl();
            "User Interface".PegiLabel().Enter_Inspect(UserInterface, ref _inspectedStuff, ++section).Nl();
            "Application".PegiLabel().Enter_Inspect(Application, ref _inspectedStuff, ++section).Nl();
        }



        #endregion
    }
}
