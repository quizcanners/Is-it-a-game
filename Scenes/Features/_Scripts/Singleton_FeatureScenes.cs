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
        [SerializeField] private CfgData _cfgData;

        public string TagForConfig => "FtrLvls";

        public void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                case "lvlls": _levelsSo.Decode(data); break;
            }
        }

        public CfgEncoder Encode()
        {
            var cody = new CfgEncoder()
                .Add("lvlls", _levelsSo);

            return cody;
        }
        public override void Inspect()
        {
            base.Inspect();
            _levelsSo.Nested_Inspect();
        }

        protected override void OnAfterEnable()
        {
            if (Application.isPlaying)
            {
                _levelsSo.Decode(_cfgData);
            }

            base.OnAfterEnable();
        }



        protected override void OnBeforeOnDisableOrEnterPlayMode(bool afterEnableCalled)
        {
            base.OnBeforeOnDisableOrEnterPlayMode(afterEnableCalled);

            _cfgData = _levelsSo.Encode().CfgData;

            if (Application.isPlaying)
            {
                _levelsSo.UnloadAll();
            }
        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_FeatureScenes))] internal class Singleton_FeatureScenesDrawer : PEGI_Inspector_Override { }
}
