using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class Persistent_PlayerData : IsItGameClassBase, IPEGI
    {
        private const string SAVE_FOLDER = "Player State";

        public Persistent_CampaignState Campaign = new();

        [SerializeField] private string _userName = "";

        public string UserName => _userName;

        private QcFile.RelativeLocation GetLocation(string userName) => new(folderName: SAVE_FOLDER, fileName: userName, asBytes: true);
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
                if (!QcFile.Load.FromPersistentPath.JsonTry(GetLocation(userName), out Persistent_PlayerData ent))
                {
                    _userName = userName;
                    return;
                }

                Game.Persistent.User = ent;

                ent._userName = userName;

            } catch(Exception ex) 
            {
                Debug.LogException(ex);
                return;
            }

            _userName = userName;
        }

        public void Clear() 
        {
            _userName = "";
        }

        #region Inspector
        private int _inspectedStuff = -1;

        public void Inspect()
        {
            int sectionIndex = -1;

            "Campaign".PegiLabel().Enter_Inspect(Campaign, ref _inspectedStuff, ++sectionIndex).Nl();

            if (_inspectedStuff == -1)
            {
                Icon.Save.Click(Save);
                Icon.Load.Click(()=> Load(UserName));
            }

        }

        #endregion
    }
}
