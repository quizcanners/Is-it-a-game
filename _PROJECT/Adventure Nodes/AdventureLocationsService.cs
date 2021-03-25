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
    public class AdventureLocationsService : IsItGameServiceBase, ITaggedCfg
    {
        public string TagForConfig => "AdvLocations";

        [SerializeField] protected DictionaryOfLocations locations = new();
        [Serializable] protected class DictionaryOfLocations : SerializableDictionary<string, AdventureNodesLocationScriptableObject> { }

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

        private int _inspectedStuff = -1;
        private int _inspectedLocation = -1;
        public override void Inspect()
        {
            pegi.nl();

            if (_inspectedStuff == -1)
            {
                if (Utils.Singleton.Get<ConfigNodesService>().AnyEntered)
                {
                    "Current Location".PegiLabel().select(ref _currentLocation, locations).nl();
                }
                else
                    "No node entered to show Configurable options".PegiLabel().writeHint();
            }

            if ("Locations".PegiLabel().isEntered(ref _inspectedStuff, 0).nl())
                "Locations".PegiLabel().edit_Dictionary(locations, ref _inspectedLocation).nl();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(AdventureLocationsService))] internal class AdventureNodesServiceDrawer : PEGI_Inspector_Override { } 
}
