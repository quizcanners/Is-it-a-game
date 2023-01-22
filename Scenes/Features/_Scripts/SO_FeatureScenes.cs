using Mono.Cecil.Cil;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]

    public class SO_FeatureScenes : ScriptableObject, IPEGI, ICfgCustom
    {
        const string FILE_NAME = "Feature Scenes";

        [SerializeField] private List<Feature> _features = new();

        [SerializeField] private pegi.CollectionInspectorMeta _collectionMeta = new("Features");

        public void UnloadAll() 
        {
            foreach (var f in _features)
                f.IsActive = false;
        }

        #region Encode & Decode

        private Dictionary<string, Feature> _decodeFeatures;

        public void DecodeInternal(CfgData data)
        {
            _decodeFeatures = new Dictionary<string, Feature>();
             
            foreach (var f in _features)
                if (f.Key.IsNullOrEmpty() == false) 
                {
                    _decodeFeatures[f.Key] = f;
                }

            this.DecodeTagsFrom(data);
        }

        public void DecodeTag(string key, CfgData data)
        {
            if (_decodeFeatures.TryGetValue(key, out var feature)) 
            {
                feature.IsActive = data.ToBool();
                _decodeFeatures.Remove(key);
            }
        }

        public CfgEncoder Encode()
        {
            CfgEncoder cody = new();

            foreach (var f in _features) 
            {
                if (!f.Key.IsNullOrEmpty())
                    cody.Add_Bool(f.Key, f.IsActive);
            }

            return cody;
        }
        #endregion

        #region Inspector
        public void Inspect()
        {
            _collectionMeta.Edit_List(_features).Nl();
        }

      
        #endregion

        [Serializable]
        internal class Feature : IPEGI_ListInspect
        {
            [SerializeField] public string Key;
            [SerializeField] private Qc_SceneInspectable _scene = new();

            public bool IsActive
            {
                get => _scene.IsLoadedOrLoading;
                set => _scene.IsLoadedOrLoading = value;
            }

            public override string ToString() => _scene.ToString();


            public void InspectInList(ref int edited, int index)
            {
                if (Key.IsNullOrEmpty() && Icon.Copy.Click("Copy Scene's Name"))
                    Key = _scene.ToString();
                "Key".PegiLabel(35).Edit(ref Key);
                _scene.InspectInList_Nested(ref edited, index);
            }
        }
    }

    [PEGI_Inspector_Override(typeof(SO_FeatureScenes))] internal class SO_FeatureScenesDrawer : PEGI_Inspector_Override { }
}
