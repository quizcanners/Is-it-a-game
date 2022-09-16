using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = Utils.QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class SO_PulseRoutesConfigurations : ScriptableObject, IPEGI //, ISerializationCallbackReceiver
    {
        public const string FILE_NAME = "Pulse Path Configs";

        public List<PulsePath> _pathConfigurations;

        [SerializeReference] private pegi.CollectionInspectorMeta pathMeta = new(FILE_NAME);

        public void Inspect()
        {
            pathMeta.Edit_List(_pathConfigurations);
        }


        /*

        private const int LOGIC_VERSION = 4;

        private PlayerPassport passport = new PlayerPassport();

        private class PlayerPassport 
        {
            public string FirstName;
        }


        #region Migration

        [SerializeField] private int Version;

        // Legacy
        [SerializeField] [System.Obsolete] private string Name;
        [SerializeField] [System.Obsolete] private string FirstName;

#       pragma warning disable CS0612 // Type or member is obsolete

        public void OnAfterDeserialize()
        {
            // Version <2 => 3
            if (Version < 3)
            {
                FirstName = Name;
            }

            // Version 3 => 4
            if (Version < 4)
            {
                passport.FirstName = FirstName;
            }
        }

#       pragma warning restore CS0612 // Type or member is obsolete

        public void OnBeforeSerialize()
        {
            Version = LOGIC_VERSION;
        }

        #endregion*/
    }
}