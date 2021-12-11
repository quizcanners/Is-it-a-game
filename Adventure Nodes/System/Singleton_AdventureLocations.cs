using QuizCanners.Inspect;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [ExecuteAlways]
    public class Singleton_AdventureLocations : IsItGameServiceBase, ITaggedCfg
    {
        public string TagForConfig => "AdvLocations";

        [SerializeField] protected DictionaryOfLocations locations = new();
        [Serializable] protected class DictionaryOfLocations : SerializableDictionary<string, SO_AdventureNodesLocation> { }

        [NonSerialized] private string _currentLocation = "";

        #region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
           .Add_String("l", _currentLocation);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                case "l": _currentLocation = data.ToString(); break;
            }
        }

        #endregion

        #region Inspector

        public override string InspectedCategory => Utils.Singleton.Categories.GAME_LOGIC;

        private pegi.EnterExitContext context = new pegi.EnterExitContext();
        private int _inspectedLocation = -1;
        public override void Inspect()
        {
            using (context.StartContext())
            {
                pegi.Nl();

                if (context.IsAnyEntered == false)
                {
                    if (Utils.Singleton.Get<Singleton_ConfigNodes>().AnyEntered)
                    {
                        "Current Location".PegiLabel().Select(ref _currentLocation, locations).Nl();
                    }
                    else
                        "No node entered to show Configurable options".PegiLabel().WriteHint();
                }

                "Locations".PegiLabel().Enter_Dictionary(locations, ref _inspectedLocation).Nl();
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_AdventureLocations))] internal class AdventureNodesServiceDrawer : PEGI_Inspector_Override { } 
}
