using QuizCanners.Inspect;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.IsItGame.Triggers;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public partial class CampaignState : IsItGameClassBase, IPEGI
    {
        public ConfigBook.Node.Reference LastSavedOnExitNode;
        [SerializeField] private string _triggerValuesJson;

        public void SaveGameFromServices() 
        {
            Singleton.Try<TriggerValuesService>(s => _triggerValuesJson = JsonUtility.ToJson(s.Values));
            Singleton.Try<ConfigNodesService>(s =>
            {
                var chain = s.CurrentChain;
                if (chain != null)
                {
                    GameEntities.Player.Campaign.LastSavedOnExitNode = s.CurrentChain.GetReferenceToLastNode();
                }
                else
                    Debug.LogError("Chain is null");
            });
        }

        public void LoadGameToServices() 
        {
            Singleton.Try<TriggerValuesService>(s => JsonUtility.FromJsonOverwrite(_triggerValuesJson, s.Values));
            Singleton.Try<ConfigNodesService>(s => s.SetCurrent(GameEntities.Player.Campaign.LastSavedOnExitNode));
        }

        private int _inspectedStuff = -1;
        private int _inspectedPlayableCharacter = -1;
        public void Inspect()
        {
            int section = -1;

            if ("Playable Characters".PegiLabel().isEntered(ref _inspectedStuff, ++section).nl())
                pegi.edit_Dictionary(PlayableCharacters, ref _inspectedPlayableCharacter).nl();

            if ("Trigger Values".PegiLabel().isEntered(ref _inspectedStuff, ++section).nl())
                Singleton.Try<TriggerValuesService>(s => s.Values.Nested_Inspect());
        }
    }
}
