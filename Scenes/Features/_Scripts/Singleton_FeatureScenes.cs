using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    [ExecuteAlways]
    public class Singleton_FeatureScenes : Singleton.BehaniourBase, ITaggedCfg
    {
        [SerializeField] private SO_FeatureScenes _levelsSo;

        public string TagForConfig => "FtrLvls";

        public void DecodeTag(string key, CfgData data)
        {

        }

        public CfgEncoder Encode() => new CfgEncoder();

        public override void Inspect()
        {
            base.Inspect();
            _levelsSo.Nested_Inspect();

        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_FeatureScenes))] internal class Singleton_FeatureScenesDrawer : PEGI_Inspector_Override { }
}
