using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    internal partial class PulseArena
    {
        [Serializable]
        internal class Unit : IGotName, IPEGI_ListInspect, IPEGI, IPEGI_Handles, IGotReadOnlyName
        {
            [SerializeField] private Point.Id _myPoint = new();
            [SerializeField] private Link.Id _link = new();
            [SerializeField] private float progress;
            [SerializeField] private CharacterSheet character = new();

            private static PulseArena GetArena() => Singleton.Get<Singleton_PulseCommander>().Data.Arena;

            internal void Update(float deltaTime) 
            {
                var start = _myPoint.GetEntity();

                if (start == null) 
                {
                    _myPoint.SetEntity(GetArena().startingPoint); 
                    return;
                }

                var dst = _link.GetEntity();

                if (dst == null) 
                {
                    _link.SetEntity(start.direction);
                    return;
                }

                if (progress < 1)
                {
                    FeetDistance dis = dst.Length;
                    GridDistance speedPerTurn = character[SpeedType.Walking];

                    float moved = (speedPerTurn.TotalFeet.ToMeters / DnDTime.SECONDS_PER_TURN) * deltaTime;
                    float portion = moved / dis.ToMeters;
                    progress = Mathf.Clamp01(progress + portion);
                }
            }

            #region Inspector
            public string NameForInspector { get => character.NameForInspector; set => character.NameForInspector = value; }

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
                            "Progress".PegiLabel(width: 60).Edit01(ref progress).Nl();
                        }
                    }

                    pegi.Enter_Inspect(character).Nl();
                }
            }

            public void InspectInList(ref int edited, int index)
            {
                this.inspect_Name();

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
                        var point = link.GetPoint(progress);
                            /*QcMath.BezierPoint(progress, 
                            p0.position,
                            p0.position + link.curve.StartVector,
                            p1.position + link.curve.EndVector,
                            p1.position
                            );*/

                        if (pegi.Handle.Button(point, Vector3.up, size: 0.2f, shape: pegi.SceneDraw.HandleCap.Cylinder))
                            _selectedInEditor = this;

                        if (this == _selectedInEditor) // progress > 0.2f && progress < 0.8f)
                        {
                            pegi.Handle.Label(this.GetNameForInspector(), point);
                        }
                    }
                }
            }

            public string GetReadOnlyName() => character.GetNameForInspector();
            #endregion
        }
    }
}