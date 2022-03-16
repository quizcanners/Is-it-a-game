using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    public partial class PulsePath
    {
        [Serializable]
        public class Unit : IPEGI_ListInspect, IPEGI, IPEGI_Handles
        {
            [SerializeField] private Point.Id _myPoint = new();
            [SerializeField] private Link.Id _link = new();
            [SerializeField] private Vector2 _linkOffsetFraction;
            [SerializeField] private float progress;

            private bool _initialized = false;

            public Vector3 GetPosition()
            {
                if (!_initialized)
                    Update(deltaTime: 0, GridDistance.FromCells(6));

                return (TryGetPath(out Link link)) ? link.GetPosition(progress, start: _myPoint, offsetFraction: _linkOffsetFraction) : Vector3.zero;
            }
            private static PulsePath GetArena() => Singleton.Get<Singleton_PulsePath>().Data;
            private bool TryGetPath(out Link link) 
            {
                link = null;
                var start = _myPoint.GetEntity();

                if (start == null)
                {
                    _myPoint.SetEntity(GetArena().startingPoint);

                    start = _myPoint.GetEntity();

                    if (start == null)
                        return false;

                    _linkOffsetFraction = UnityEngine.Random.insideUnitCircle;
                }

                link = _link.GetEntity();

                if (link == null)
                {
                    _link.SetEntity(start.direction);

                    link = _link.GetEntity();
                }

                return link != null;
            }
            internal void Update(float deltaTime, GridDistance speedPerTurn) 
            {
                _initialized = true;

                if (!TryGetPath(out var path))
                    return;

                if (progress < 1)
                {
                    FeetDistance dis = path.Length;
                
                    float moved = (speedPerTurn.TotalFeet.ToMeters / DnDTime.SECONDS_PER_TURN) * deltaTime;
                    float portion = moved / dis.ToMeters;
                    progress = Mathf.Clamp01(progress + portion);
                } else 
                {
                    var newPoint = path.End;

                    var newPath = newPoint.direction.GetEntity();

                    if (newPath != null)
                    {
                        _myPoint = new Point.Id(newPoint);
                        progress = 0;
                        _link = new Link.Id();
                    }
                }
            }

            #region Inspector

            [SerializeField] private pegi.EnterExitContext context = new();

            private static Unit _selectedInEditor;

            public void Inspect()
            {
                using (context.StartContext())
                {
                    if (context.IsAnyEntered == false)
                    {
                        "From".PegiLabel(50).Write();

                        _myPoint.InspectSelectPart().Nl();//InspectInList_Nested().Nl();

                        var p = _myPoint.GetEntity();

                        if (p != null)
                        {
                            var lnks = p.GetLinks();
                            Link l = _link.GetEntity();
                            if ("Link".PegiLabel(50).Select(ref l, lnks).Nl())
                                _link.SetEntity(l);
                        }

                        var lnk = _link.GetEntity();
                        if (lnk != null)
                        {
                            "Progress".PegiLabel(width: 60).Edit_01(ref progress).Nl();
                        }
                    }
                }
            }

            public void InspectInList(ref int edited, int index)
            {
                "Progress".PegiLabel(width: 60).Edit_01(ref progress).Nl();

                if (Icon.Enter.Click())
                    edited = index;
            }

            public void OnSceneDraw()
            {
                if (_link.TryGetEntity(out Link link)) 
                {
                    var p0 = link.Start;
                    var p1 = link.End;
                    if (p0 != null && p1 != null) 
                    {
                        var point = GetPosition();

                        if (pegi.Handle.Button(point, Vector3.up, size: 0.2f, shape: pegi.SceneDraw.HandleCap.Cylinder))
                            _selectedInEditor = this;

                        if (this == _selectedInEditor) // progress > 0.2f && progress < 0.8f)
                        {
                            pegi.Handle.Label(this.GetNameForInspector(), point);
                        }
                    }
                }
            }

            #endregion

        }
    }
}