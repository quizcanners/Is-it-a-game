using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    
    public class SO_GameLevels : ScriptableObject, IPEGI
    {
        const string FILE_NAME = "Game Levels";

        [SerializeField] internal List<GameLevel> _levels = new();

        internal void SetLevel(int index)
        {
            if (_levels.Count > index && index >= 0)
                SetLevel(_levels[index]);
        }

        internal void SetLevel(GameLevel level)
        {
            foreach (var l in _levels)
            {
                l.IsCurrentLevel = l == level;
            }
        }

        public int GetCurrentLevelIndex() 
        {
            for (int i =0; i<_levels.Count; i++) 
            {
                if (_levels[i].IsCurrentLevel)
                    return i;
            }

            return -1;
        }

        #region Inspector
        private readonly pegi.CollectionInspectorMeta _collectionInspector = new("Levels");

        public void Inspect()
        {
            "Encoded to Config ased on Index".PegiLabel().WriteWarning().Nl();
            _collectionInspector.Edit_List(_levels).Nl();
        }

        #endregion

        [Serializable]
        internal class GameLevel : IPEGI_ListInspect
        {
            [SerializeField] private Qc_SceneInspectable _scene = new();

            public bool IsCurrentLevel
            {
                get => _scene.IsLoadedOrLoading;
                set => _scene.IsLoadedOrLoading = value;
            }

            public override string ToString() => _scene.ToString();

            public void InspectInList(ref int edited, int index)
            {
                if (_scene.InspectInList_Nested(ref edited, index) && _scene.IsLoadedOrLoading)
                {
                    Singleton.Try<Singleton_GameLevels>(s => s.SetLevel(this));
                }
            }
        }
    }

    [PEGI_Inspector_Override(typeof(SO_GameLevels))] internal class SO_GameLevelsDrawer : PEGI_Inspector_Override { }
}