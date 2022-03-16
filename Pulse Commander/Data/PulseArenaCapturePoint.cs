using Dungeons_and_Dragons.Tables;
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
        internal class Point : IGotName, IPEGI_ListInspect, IPEGI, IPEGI_Handles
        {
            [SerializeField] public float Radius = 1;
            [SerializeField] public bool Explored;
           
            [SerializeField] internal Vector3 position;
            [SerializeField] internal Link.Id direction = new();
            [SerializeField] internal TableRollResult rollResult = new();

            [SerializeField] private string _name = "";
            [SerializeField] private List<LocationInstancer> locations = new();

            private static PulsePath GetArena() => Singleton.Get<Singleton_PulsePath>().Data;

            public List<Link> GetLinks() 
            {
                var allLinks = GetArena().links;
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
                if (Singleton_PulsePath.DrawCurves)
                {
                    if (pegi.Handle.FreeMove(position, out var newPos))
                        position = newPos;
                } else 
                {
                    pegi.Handle.Position(position, out var newPos).OnChanged(()=> position = newPos);
                }

                pegi.Handle.Label(_name.IsNullOrEmpty() ? rollResult.GetNameForInspector() : _name, position, offset: Vector3.up * 2);

                if (direction.TryGetEntity(out var dir)) 
                {
                    pegi.Gizmo.Ray(position, dir.curve.StartVector);
                }
            }


            [SerializeField] private pegi.EnterExitContext context = new ();
            [SerializeField] private pegi.CollectionInspectorMeta locationsMeta = new();

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

                    locationsMeta.Enter_List(locations).Nl();
                    rollResult.Enter_Inspect_AsList();
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

            #region Locations Base

            [Serializable] private class LocationInstancer : TypedInstance.JsonSerializable<LocationBase> { }

            private abstract class LocationBase : IGotReadOnlyName, IPEGI
            {
                public virtual string GetReadOnlyName() => GetType().Name;

                public virtual void Inspect()
                {
                    GetReadOnlyName().PegiLabel().Nl();
                }
            }

            #endregion

            #region Locations

            private class Tavern : LocationBase 
            {
                
            }

            private class Guild : LocationBase , IPEGI_ListInspect
            {
                [SerializeField] private GuildType type;

                public void InspectInList(ref int edited, int index)
                {
                    "Guild Type".PegiLabel(90).Edit_Enum(ref type);

                    if (Icon.Enter.Click())
                        edited = index;
                }

                public enum GuildType 
                {
                    Thieves, Assassins, Traders, Mages
                }
            }

            private class Market : LocationBase 
            {
                
            }

            #endregion

            [Serializable]
            public class Id : SmartStringIdGeneric<Point>
            {
                internal Id() { }
                public Id(Point point) { SetEntity(point); }
                public Id(Id point) { Id = point.Id; }

                protected override Dictionary<string, Point> GetEnities() => GetArena().points;
            }

            [Serializable]
            public class SerializableDictionary : SerializableDictionary<string, Point> { }
        }
    }
}