using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class ScenesService : IsItGameServiceBase, INeedAttention, Singleton.ILoadingProgressForInspector
    {
        [SerializeField] private SceneManagerScriptableObject _scenes;

        public bool IsLoadedAndInitialized(IigEnum_Scene scene) => _scenes.IsFullyLoadedAndInitialized(scene); //.TryGet(scene, out var match) && match.IsLoadedFully;
        public bool IsLoadedOrLoading(IigEnum_Scene scene) => _scenes[scene];
        public bool SetIsLoading(IigEnum_Scene scene, bool value) => _scenes[scene] = value;

        public void Update()
        {
            if (TryEnterIfStateChanged()) 
            {
                _scenes.LoadAndUnloadOthers(GameStateMachine.Get<IigEnum_Scene>());
            }
        }

        public bool IsLoading(ref string state, ref float progress01)
        {
            if (_scenes)
                return _scenes.IsLoading(ref state, ref progress01);

            return false;
        }

        #region Inspector

        public override string InspectedCategory => Utils.Singleton.Categories.SCENE_MGMT;

        private int _inspectedStuff = -1;

        public override void Inspect()
        {
            pegi.nl();
            "Scenes".PegiLabel().edit_enter_Inspect(ref _scenes, ref _inspectedStuff, 0).nl();
        }

        public override string NeedAttention()
        {
            if (!_scenes)
                return "Scenes not assigned";

            var na = _scenes.NeedAttention();

            if (!na.IsNullOrEmpty())
                return na;

            return base.NeedAttention();
            
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(ScenesService))] internal class ScenesServiceDrawer : PEGI_Inspector_Override { }
}