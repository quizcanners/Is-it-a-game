using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    public partial class PulsePath
    {
        [Serializable]
        internal class Link : IPEGI_ListInspect, IPEGI_Handles, IGotReadOnlyName, IPEGI
        {
            [SerializeField] private Point.Id _start = new();
            [SerializeField] private Point.Id _end = new();
            [SerializeField] internal BezierCurve curve = new();

            private bool _paintCurve = false;

            public float Width 
            { 
                get 
                {
                    var s = Start;
                    var e = End;

                    if (s!= null && e != null) 
                    {
                        return Mathf.Min(s.Radius, e.Radius) * 2;
                    }

                    return 2;
                } 
            }

            public Point Start => _start.GetEntity();
            public Point End => _end.GetEntity();

            public Vector3 GetPosition(float progress, Point.Id start, Vector2 offsetFraction = default)
            {
                var s =  Start;
                var e =  End;

                if (s == null || e == null)
                    return Vector3.zero;

                bool fromStart = _start.SameAs(start);

                if (!fromStart)
                    progress = 1 - progress;

                var pos = curve.GetPoint(progress, s.position, e.position);

                if (offsetFraction.magnitude > 0)
                {
                    float theWidth = Mathf.Lerp(s.Radius, e.Radius, progress);
                    pos += offsetFraction.ToVector3XZ() * theWidth;
                }

                return pos;
            }

            public bool Contains(Point point) =>
                (_start.TryGetEntity(out var entA) && entA == point) ||
                (_end.TryGetEntity(out var entB) && entB == point);

            private static PulsePath GetArena() => Singleton.Get<Singleton_PulsePath>().Data;

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

                Icon.Swap.Click().OnChanged(()=> 
                { 
                    var tmp = _end; 
                    _end = _start;
                    _start = tmp;

                    curve.SwapVectors();
                });

                _end.InspectSelectPart();

                if (Icon.Enter.Click())
                    edited = index;
            }

            public void OnSceneDraw()
            {
                if (Start != null && End != null)
                {
                    if (Singleton_PulsePath.DrawCurves)
                        pegi.Handle.Bazier(curve, Start.position, End.position, Color.white, width: Width);
                    else
                        pegi.Handle.Line(Start.position, End.position, Color.white, thickness: Width);


                    if (_paintCurve)
                        curve.OnSceneDraw();

                } 
            }

            public string GetReadOnlyName() => "{0} -> {1}".F(Start.GetNameForInspector(), End.GetNameForInspector());

            public void Inspect()
            {
                "Paint Curve".PegiLabel().ToggleIcon(ref _paintCurve).Nl();
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
