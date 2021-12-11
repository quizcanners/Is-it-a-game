using QuizCanners.Inspect;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.IsItGame.Triggers;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public partial class Persistent_CampaignState : IsItGameClassBase, IPEGI
    {
        public SO_ConfigBook.Node.Reference LastSavedOnExitNode;
        [SerializeField] private string _triggerValuesJson;

        public void SaveGameFromServices() 
        {
            Singleton.Try<Singleton_TriggerValues>(s => _triggerValuesJson = JsonUtility.ToJson(s.Values));
            Singleton.Try<Singleton_ConfigNodes>(s =>
            {
                var chain = s.CurrentChain;
                if (chain != null)
                {
                    Game.Persistent.User.Campaign.LastSavedOnExitNode = s.CurrentChain.GetReferenceToLastNode();
                }
                else
                    Debug.LogError("Chain is null");
            });
        }

        public void LoadGameToServices() 
        {
            Singleton.Try<Singleton_TriggerValues>(s => JsonUtility.FromJsonOverwrite(_triggerValuesJson, s.Values));
            Singleton.Try<Singleton_ConfigNodes>(s => s.SetCurrent(Game.Persistent.User.Campaign.LastSavedOnExitNode));
        }

        private pegi.EnterExitContext context = new pegi.EnterExitContext();
        private int _inspectedPlayableCharacter = -1;
        public void Inspect()
        {
            using (context.StartContext())
            {
                "Playable Characters".PegiLabel().Enter_Dictionary(PlayableCharacters, ref _inspectedPlayableCharacter).Nl();

                "Trigger Values".PegiLabel().IsEntered().Nl().If_Entered(()=>
                    Singleton.Try<Singleton_TriggerValues>(s => s.Values.Nested_Inspect()));
            }
        }
    }
}
