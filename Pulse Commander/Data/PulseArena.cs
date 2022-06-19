using QuizCanners.Inspect;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    [Serializable]
    public partial class PulsePath : IPEGI, IPEGI_Handles, IGotName
    {
        [SerializeField] internal Point.Id startingPoint = new();
        [SerializeField] internal Point.SerializableDictionary points = new();
        [SerializeField] internal List<Link> links = new();
        [SerializeField] internal List<Unit> testUnits = new();

        [SerializeField] private string _name = "Pulse Arena";

        internal void Update(float deltaTime) 
        {
            foreach (var u in testUnits)
                u.Update(deltaTime, Dungeons_and_Dragons.GridDistance.FromCells(6));
        }

        #region Inspector

        [SerializeField] private pegi.EnterExitContext conext = new();
        [SerializeField] private pegi.CollectionInspectorMeta _linksMeta = new("Links");
        [SerializeField] private pegi.CollectionInspectorMeta _pointMeta = new("Points");
        [SerializeField] private pegi.CollectionInspectorMeta _unitsMeta = new("Test Units");

        public string NameForInspector { get => _name; set => _name = value; }

        public void Inspect()
        {
            using (conext.StartContext())
            {
                if (!conext.IsAnyEntered)
                {
                    "Starting point".PegiLabel(90).Write();
                    startingPoint.InspectSelectPart().Nl();

                    "Skip Time 1s".PegiLabel().Click().Nl().OnChanged(() => Update(1));
                }

                _pointMeta.Enter_Dictionary(points).Nl();
                _linksMeta.Enter_List(links).Nl();
                _unitsMeta.Enter_List(testUnits).Nl();
            }
        }

        public void OnSceneDraw()
        {
            foreach (var p in points)
                p.Value.OnSceneDraw();

            foreach (var l in links)
                l.OnSceneDraw();

            foreach (var u in testUnits)
                u.OnSceneDraw();
        }
        #endregion
    }
}