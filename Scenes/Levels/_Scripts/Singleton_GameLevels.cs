using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;
using static QuizCanners.IsItGame.SO_GameLevels;

namespace QuizCanners.IsItGame
{

    [ExecuteAlways]
    public class Singleton_GameLevels : Singleton.BehaniourBase, ITaggedCfg
    {
        [SerializeField] private SO_GameLevels _levelsSo;

        internal void SetLevel(GameLevel level)
        {
            _levelsSo.SetLevel(level);
        }

        public override void Inspect()
        {
            base.Inspect();
            pegi.Nl();
            "Levels".PegiLabel().Edit_Inspect(ref _levelsSo).Nl();
        }

        #region Encode & Decode
        public string TagForConfig => "Game Levels";
        public CfgEncoder Encode() => new CfgEncoder().Add("lv", _levelsSo.GetCurrentLevelIndex());

        public void DecodeTag(string key, CfgData data)
        {
            switch (key)
            {
                case "lv": _levelsSo.SetLevel(data.ToInt()); break;
            }
        }
        #endregion


    }

    [PEGI_Inspector_Override(typeof(Singleton_GameLevels))] internal class Singleton_GameLevelsDrawer : PEGI_Inspector_Override { }
}