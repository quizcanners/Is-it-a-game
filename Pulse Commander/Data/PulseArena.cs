using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    [Serializable]
    internal partial class PulseArena : IPEGI, IGotReadOnlyName, IPEGI_Handles
    {
        [SerializeField] internal Point.Id startingPoint = new();
        [SerializeField] internal List<Point> points = new();
        [SerializeField] internal List<Link> links = new();
        [SerializeField] internal List<Unit> units = new();

        internal void Update(float deltaTime) 
        {
            foreach (var p in points)
                p.Update(deltaTime);

            foreach (var u in units)
                u.Update(deltaTime);
        }

        #region Inspector
        public string GetReadOnlyName() => "Pulse Arena";

        [SerializeField] private pegi.EnterExitContext conext = new();
        [SerializeField] private pegi.CollectionInspectorMeta _linksMeta = new("Links");
        [SerializeField] private pegi.CollectionInspectorMeta _pointMeta = new("Points");
        [SerializeField] private pegi.CollectionInspectorMeta _unitsMeta = new("Units");

        public void Inspect()
        {
            using (conext.StartContext())
            {
                if (!conext.IsAnyEntered)
                {
                    "Starting point".PegiLabel(90).Write();
                    startingPoint.InspectSelectPart().Nl();
                }

                _pointMeta.Enter_List(points).Nl();
                _linksMeta.Enter_List(links).Nl();
                _unitsMeta.Enter_List(units).Nl();
            }
        }

        public void OnSceneDraw()
        {
            foreach (var p in points)
                p.OnSceneDraw();

            foreach (var l in links)
                l.OnSceneDraw();

            foreach (var u in units)
                u.OnSceneDraw();
        }
        #endregion
    }
}