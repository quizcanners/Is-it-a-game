using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerEntity : IsItGameClassBase, IPEGI
    {
        private const string SAVE_FOLDER = "Player State";

        public CampaignState Campaign = new CampaignState();

        [SerializeField] private string _userName = "";

        public string UserName => _userName;

        private QcFile.RelativeLocation GetLocation(string userName) => new QcFile.RelativeLocation(folderName: SAVE_FOLDER, fileName: userName, asBytes: true);
        public void Save()
        {
            if (_userName.IsNullOrEmpty() == false)
                QcFile.Save.ToPersistentPath.JsonTry(objectToSerialize: this, GetLocation(UserName));
        }
        public void Delete()
        {
            QcFile.Delete.InPersistentFolder.FileTry(GetLocation(UserName));
            _userName = "";
        }
        public void Load(string userName)
        {
            try
            {
                if (!QcFile.Load.FromPersistentPath.JsonTry(GetLocation(userName), out PlayerEntity ent))
                {
                    Debug.LogError("Couldn't load " + userName);
                    _userName = userName;
                    return;
                }

                GameEntities.Player = ent;

                ent._userName = userName;

            } catch(Exception ex) 
            {
                Debug.LogException(ex);
                return;
            }

            _userName = userName;
        }

        #region Inspector
        private int _inspectedStuff = -1;

        public void Inspect()
        {
            int sectionIndex = -1;

            "Campaign".PegiLabel().enter_Inspect(Campaign, ref _inspectedStuff, ++sectionIndex).nl();

            if (_inspectedStuff == -1)
            {
                icon.Save.Click(Save);
                icon.Load.Click(()=> Load(UserName));
            }
        }

        #endregion
    }
}
