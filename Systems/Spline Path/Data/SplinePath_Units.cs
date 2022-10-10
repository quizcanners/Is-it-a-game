using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizCanners.IsItGame.SplinePath
{
    public partial class SO_SplinePath
    {
      

        [Serializable]
        public class Unit : IPEGI_ListInspect, IPEGI, IPEGI_Handles
        {
            [SerializeField] public float Progress;
            [SerializeField] internal Point.Id startPoint = new();
            [SerializeField] private Link.Id _link = new();
            [SerializeField] internal Vector2 LinkOffsetFraction;
           
            [SerializeField] public bool isPlayer;

            internal float previousDirection;

            private bool _initialized = false;

            public Vector3 GetPosition()
            {
                if (!_initialized)
                    Update(deltaTime: 0, GridDistance.FromCells(6));

                return (TryGetPath(out Link link)) ? link.GetPosition(this) : Vector3.zero;
            }
            
            private bool TryGetPath(out Link link) 
            {
             
                Point start = startPoint.GetEntity();

                if (start == null)
                {
                    var cfg = Singleton_SplinePath.CurrentCfg;


                    if (cfg)
                    {
                        startPoint.SetEntity(isPlayer ? cfg.playerStartingPoint : cfg.enemyStartingPoint);
                    }
                    start = startPoint.GetEntity();

                    if (start == null)
                    {
                        link = null;
                        return false;
                    }

                    LinkOffsetFraction = UnityEngine.Random.insideUnitCircle;
                }

                link = _link.GetEntity();

                if (link == null)
                {
                    link = start.direction.GetEntity();
                    if (link != null)
                    {
                        _link.SetEntity(link);
                    } else 
                    {
                        var anyLink = start.GetLinks().FirstOrDefault();
                        if (anyLink != null)
                        {
                            _link.SetEntity(anyLink);
                            link = anyLink;
                        }
                    }
                }

                return link != null;
            }
            internal void Update(float deltaTime, GridDistance speedPerTurn) 
            {
                _initialized = true;

                if (!TryGetPath(out var path))
                    return;

                float moved = (speedPerTurn.TotalFeet.ToMeters / DnDTime.SECONDS_PER_TURN) * deltaTime;

                int maxIterations = 100;

                while (moved > 0)
                {
                    maxIterations--;
                    if (maxIterations < 0) 
                    {
                        Debug.LogError("Infinite motion loop");
                        break;
                    }

                    float linkLength = path.Length.ToMeters;

                    Progress += moved / linkLength;

                    if (Progress <= 1)
                    {
                        break;
                    }
                    else
                    {
                        moved = (Progress - 1) * linkLength;
                        Progress = 0;

                        Point.Id newPointId = path.GetDifferentFrom(startPoint);
                        startPoint = newPointId;

                        Link.Id newPath = newPointId.GetEntity().direction;
                        _link = newPath;

                        if (!TryGetPath(out path))
                            return;
                    }
                }
            }

            public bool TryMove (Vector3 vector) 
            {
                if (!_initialized)
                    Update(deltaTime: 0, GridDistance.FromCells(6));

                if (_link == null)
                    return false;

                Link currentLink = _link.GetEntity();

                if (currentLink == null)
                    return false;
                
                currentLink.Move(this, vector, out float leftoverFraction);

                if (leftoverFraction > 0) 
                {
                    if (Progress > 0.99f)
                    {
                        startPoint = currentLink.GetDifferentFrom(startPoint);
                        Progress = 0;
                    }

                    previousDirection = 1;

                    var links = startPoint.GetEntity().GetLinks();

                    if (links.Count > 1)
                    {
                        float closestDot = -10;

                        foreach (Link potentialLink in links)
                        {
                            if (currentLink == potentialLink)
                                continue;

                            float dot = Vector3.Dot(potentialLink.GetNormal(this), vector);

                            if (closestDot < dot)
                            {
                                closestDot = dot;
                                currentLink = potentialLink;
                            }
                        }

                        _link.SetEntity(currentLink);

                        vector *= leftoverFraction;

                        currentLink.Move(this, vector, out _);
                        
                    }

                }

                return true;
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

                        startPoint.InspectSelectPart().Nl();//InspectInList_Nested().Nl();

                        var p = startPoint.GetEntity();

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
                            "Progress".PegiLabel(width: 60).Edit_01(ref Progress).Nl();
                        }
                    }
                }
            }

            public void InspectInList(ref int edited, int index)
            {
                "Progress".PegiLabel(width: 60).Edit_01(ref Progress).Nl();

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