using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class PersistentGameStateData : ScriptableObject, IPEGI, ICfg
    {
        public const string FILE_NAME = "Game States";

        public UserInterfaceEntity UserInterface = new UserInterfaceEntity(); // Ui
        public ApplicationEntity Application = new ApplicationEntity(); // All users

        public PlayerEntity Player = new PlayerEntity();  // Specific user

        [SerializeField] private CfgData _cfg;
        private readonly QcFile.RelativeLocation _saveLocation = new QcFile.RelativeLocation(folderName: "Data", fileName: FILE_NAME, asBytes: false);

        #region Saving & Loading

        public CfgEncoder Encode() => new CfgEncoder();
        public void DecodeTag(string key, CfgData data)
        {
            /*  switch (key) 
              {

              }*/
        }

        public void Save()
        {
            _cfg = Encode().CfgData;
            QcFile.Save.ToPersistentPath.JsonTry(objectToSerialize: this, _saveLocation);
        }
        public void Load()
        {
            var tmp = this;
            QcFile.Load.FromPersistentPath.TryOverrideFromJson(_saveLocation, ref tmp);
            this.Decode(_cfg);
        }

        #endregion

        #region Inspector
        private int _inspectedStuff = -1;
        public void Inspect()
        {
            if (_inspectedStuff == -1)
            {
                string tmp = Player.UserName;
                if (icon.Delete.ClickConfirm(confirmationTag: "delUsr", toolTip: "Delete this User?")) 
                {
                    Application.AvailableUsers.Remove(tmp);
                    Player.Delete();
                }

                if ("User".PegiLabel(60).select(ref tmp, Application.AvailableUsers))
                {
                    Player.Save();
                    Player.Load(tmp);
                }

                icon.Folder.Click(()=> QcFile.Explorer.OpenPersistentFolder()).nl();
            }

            int section = -1;

            "Player".PegiLabel().enter_Inspect(Player,                      ref _inspectedStuff, ++section).nl();
            "User Interface".PegiLabel().enter_Inspect(UserInterface,       ref _inspectedStuff, ++section).nl();
            "Application".PegiLabel().enter_Inspect(Application,            ref _inspectedStuff, ++section).nl();
           // "Values".PegiLabel().enter_Inspect(NodeNotes.TriggerValues.Global,     ref _inspectedStuff, ++section).nl();
        }


       
        #endregion
    }
}