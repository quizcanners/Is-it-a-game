using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace QuizCanners.IsItGame.SplinePath
{
    public partial class SO_SplinePath
    {
        [Serializable]
        internal class Point : IGotName, IPEGI_ListInspect, IPEGI, IPEGI_Handles
        {
            [SerializeField] public float Radius = 1;
            [SerializeField] public bool Explored;
           
            [SerializeField] internal Vector3 position;
            [SerializeField] internal Link.Id direction = new();

            [SerializeField] private string _name = "";
            [SerializeField] private bool _overrideNode;
            [SerializeField] private NodeNotes.SO_ConfigBook.Node.Id nodeId = new NodeNotes.SO_ConfigBook.Node.Id();

            public List<Link> GetLinks() 
            {
                var allLinks = Singleton_SplinePath.CurrentCfg.links;
                var filtered = new List<Link>();

                foreach (var l in allLinks)
                    if (l.Contains(this))
                        filtered.Add(l);

                return filtered;
            }

            #region Inspector
            public string NameForInspector
            {
                get => _name;
                set => _name = value;
            }

            public void OnSceneDraw()
            {
                if (Singleton_SplinePath.DrawCurves)
                {
                    if (pegi.Handle.FreeMove(position, out var newPos))
                        position = newPos;
                } else 
                {
                    pegi.Handle.Position(position, out var newPos).OnChanged(()=> position = newPos);
                }

                pegi.Handle.Label(_name, position, offset: Vector3.up * 2);

                if (direction.TryGetEntity(out var dir)) 
                {
                    pegi.Gizmo.Ray(position, dir.curve.StartVector);
                }
            }

            [SerializeField] private pegi.EnterExitContext context = new ();

            public void Inspect()
            {
                using (context.StartContext())
                {
                    if (context.IsAnyEntered == false)
                    {
                        "Explored".PegiLabel(60).ToggleIcon(ref Explored).Nl();
                        "Position".PegiLabel(60).Edit(ref position).Nl();
                        "Radius".PegiLabel(60).Edit(ref Radius).Nl();

                        var lnk = direction.GetEntity();
                        if ("Direction".PegiLabel(60).Select(ref lnk, GetLinks()).Nl())
                            direction.SetEntity(lnk);
                    }

                    if (context.IsAnyEntered == false)
                        "Node".PegiLabel().ToggleIcon(ref _overrideNode, hideTextWhenTrue: true);

                    if (_overrideNode)
                        nodeId.Enter_Inspect().Nl();
                }
            }

            public void InspectInList(ref int edited, int index)
            {
                this.inspect_Name();

                var lnk = direction.GetEntity();
                "->".PegiLabel(30).Select(ref lnk, GetLinks()).OnChanged(()=> direction.SetEntity(lnk));

                if (Icon.Enter.Click())
                    edited = index;
            }
            #endregion


            [Serializable]
            public class Id : SmartStringIdGeneric<Point>
            {
                public Id() { }
                public Id(Point point)
                {
                    SetEntity(point);
                }
                protected override Dictionary<string, Point> GetEnities()
                {
                    var cur = Singleton_SplinePath.CurrentCfg;
                    
                    if (cur)
                        return cur.points;
                    return null;
                }
            }
   

            [Serializable]
            public class SerializableDictionary : SerializableDictionary<string, Point> { }
        }
    }
}