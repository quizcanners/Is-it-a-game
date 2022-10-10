using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.SplinePath
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = Utils.QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public partial class SO_SplinePath : ScriptableObject, IPEGI, IPEGI_Handles, IGotName
    {
        public const string FILE_NAME = "Pulse Path Configs";

        [SerializeField] internal Point.Id enemyStartingPoint = new();
        [SerializeField] internal Point.Id playerStartingPoint = new();
        [SerializeField] internal Point.SerializableDictionary points = new();
        [SerializeField] internal List<Link> links = new();

        [SerializeField] private string _name = "Pulse Arena";

        private static Singleton_SplinePath Mgmt => Singleton.Get<Singleton_SplinePath>();

        #region Inspector
        [SerializeField] private pegi.EnterExitContext conext = new();
        [SerializeField] private pegi.CollectionInspectorMeta _linksMeta = new("Links");
        [SerializeField] private pegi.CollectionInspectorMeta _pointMeta = new("Points");

        public string NameForInspector { get => _name; set => _name = value; }

        public void Inspect()
        {
            using (conext.StartContext())
            {
                if (!conext.IsAnyEntered)
                {
                    "Enemy Start Point".PegiLabel(120).Write();
                    enemyStartingPoint.InspectSelectPart().Nl();

                    "Player Starting Point".PegiLabel(120).Write();
                    playerStartingPoint.InspectSelectPart().Nl();
                }

                _pointMeta.Enter_Dictionary(points).Nl();
                _linksMeta.Enter_List(links).Nl();
            }
        }

        public void OnSceneDraw()
        {
            foreach (var p in points)
                p.Value.OnSceneDraw();

            foreach (var l in links)
                l.OnSceneDraw();
        }
        #endregion
    }
}