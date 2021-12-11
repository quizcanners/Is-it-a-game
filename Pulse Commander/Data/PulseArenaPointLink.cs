using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    internal partial class PulseArena
    {
        [Serializable]
        internal class Link : IPEGI_ListInspect, IPEGI_Handles, IGotReadOnlyName, IPEGI
        {
            [SerializeField] private Point.Id _start = new();
            [SerializeField] private Point.Id _end = new();
            [SerializeField] internal BezierCurve curve = new();
            [SerializeField] internal int width = 1;

            public int Capacity => width * width;

            public Point Start => _start.GetEntity();
            public Point End => _end.GetEntity();

            public Vector3 GetPoint(float progress)
            {
                var s = Start;
                var e = End;

                if (s == null || e == null)
                    return Vector3.zero;

                return curve.GetPoint(progress, s.position, e.position);
            }

            public bool Contains(Point point) =>
                (_start.TryGetEntity(out var entA) && entA == point) ||
                (_end.TryGetEntity(out var entB) && entB == point);

            internal void Update(float deltaTime)
            {

            }

            private static PulseArena GetArena() => Singleton.Get<Singleton_PulseCommander>().Data.Arena;

            private readonly Gate.Vector3Value _positionDirty = new();
            private FeetDistance feetDistance = new();
            public FeetDistance Length 
            {
                get 
                {
                    if (Start == null || End == null) 
                    {
                        return feetDistance;    
                    }

                    var a = Start.position;
                    var b = End.position;

                    if ( _positionDirty.TryChange(a + b)) 
                    {
                        feetDistance = FeetDistance.FromMeters(curve.CalculateLength(start: a, end: b));
                    }

                    return feetDistance;
                }
            }

            #region Inspector

            public void InspectInList(ref int edited, int index)
            {
                _start.InspectSelectPart();
                _end.InspectSelectPart();

                if (Icon.Enter.Click())
                    edited = index;
            }

            public void OnSceneDraw()
            {
                if (Start != null && End != null)
                {
                    if (Singleton_PulseCommander.DrawCurves)
                        pegi.Handle.Bazier(curve, Start.position, End.position, Color.white, width: width);
                    else
                        pegi.Handle.Line(Start.position, End.position, Color.white, thickness: width);


                    curve.OnSceneDraw();

                } 
            }

            public string GetReadOnlyName() => "{0} -> {1}".F(Start.GetNameForInspector(), End.GetNameForInspector());

            public void Inspect()
            {
                "Width".PegiLabel(60).Edit(ref width, 1, 10).Nl();

            }

            #endregion

            [Serializable]
            public class Id : SmartIntIdGeneric<Link>
            {
                public Id() { } 
                public Id(Link link) 
                {
                    SetEntity(link);
                }
                protected override List<Link> GetEnities() => GetArena().links;
            }

        }
    }
}
