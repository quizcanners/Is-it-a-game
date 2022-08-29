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

            public Vector3 GetPosition(Unit unit)
            {
                if (!TryGetPoints(unit, out Point start, out Point end, out bool swapped))
                {
                    return Vector3.zero;
                }

                var pos = curve.GetPoint(unit.Progress, start.position, end.position, inverted: swapped);

                if (unit.LinkOffsetFraction.magnitude > 0)
                {
                    float theWidth = Mathf.Lerp(start.Radius, end.Radius, swapped ? 1- unit.Progress : unit.Progress);
                    pos += unit.LinkOffsetFraction.ToVector3XZ() * theWidth;
                }

                return pos;
            }

            public Vector3 GetNormal(Unit unit) 
            {
                bool swapped = !unit.startPoint.Equals(_start);

                return curve.GetNormal(unit.Progress, Start.position, End.position, inverted: swapped);
            }

            public void Move(Unit unit, Vector3 vector, out float leftoverFraction) 
            {
                if (!TryGetPoints(unit, out Point start, out Point end, out bool swapped)) 
                {
                    leftoverFraction = 1;
                    return;
                }

                var normal = curve.GetNormal(unit.Progress, start.position, end.position, inverted: swapped);

                float forward = Vector3.Dot(normal, vector.normalized);

                float direction;

              ///  if (unit.previousDirection == 0 || Mathf.Abs(forward) > 0.33f)
                //{
                    direction = forward > 0f ? 1f : -1f;
                    unit.previousDirection = direction;
              //  }
              //  else
                  //  direction = unit.previousDirection;

                if (swapped)
                    direction = -direction;

                float moveAmount =  (vector.magnitude / Length.ToMeters);

                unit.Progress += direction * moveAmount;

                if (unit.Progress < 0 || unit.Progress > 1) 
                {
                    float extra = Mathf.Abs(unit.Progress - 0.5f) - 0.5f;

                    leftoverFraction = Mathf.Abs(extra/moveAmount);

                    unit.Progress = Mathf.Clamp01(unit.Progress);
                }
                else 
                {
                    leftoverFraction = 0;
                }
            }

            public bool Contains(Point point) =>
                (_start.TryGetEntity(out var entA) && entA == point) ||
                (_end.TryGetEntity(out var entB) && entB == point);

            private bool TryGetPoints(Unit unit, out Point start, out Point end, out bool swapped)
            {
                start = Start;
                end = End;

                if (start == null || end == null)
                {
                    swapped = false;
                    return false;
                }

                swapped = !_start.Equals(unit.startPoint);

                return true;
            }

       

            public Point.Id GetDifferentFrom(Point.Id point) => point.Equals(_start) ? _end : _start;


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
