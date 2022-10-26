using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]

    public class SO_FeatureScenes : ScriptableObject, IPEGI, ICfg
    {
        const string FILE_NAME = "Feature Scenes";

        [SerializeField] private List<Feature> _features = new();

        [SerializeField] private pegi.CollectionInspectorMeta _collectionMeta = new("Features");

        #region Encode & Decode
        public void DecodeTag(string key, CfgData data)
        {

        }

        public CfgEncoder Encode() => new CfgEncoder();
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
            [SerializeField] private Qc_SceneInspectable _scene = new();

            public bool IsActive
            {
                get => _scene.IsLoadedOrLoading;
                set => _scene.IsLoadedOrLoading = value;
            }

            public override string ToString() => _scene.ToString();

            public void InspectInList(ref int edited, int index)
            {
                _scene.InspectInList_Nested(ref edited, index);
            }
        }
    }

    [PEGI_Inspector_Override(typeof(SO_FeatureScenes))] internal class SO_FeatureScenesDrawer : PEGI_Inspector_Override { }
}
