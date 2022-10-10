using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.SplinePath
{
    [ExecuteAlways]
    public class Singleton_SplinePath : Singleton.BehaniourBase, IPEGI, IPEGI_Handles
    {
        internal static List<SplinePath_Instance> Instances => SplinePath_Instance.s_Instances;

        internal static SO_SplinePath CurrentCfg
        {
            get
            {
                var inst = Instances.TryGet(Instances.Count - 1);
                if (inst)
                    return inst.config;
                return null;
            }
        }

        internal static bool DrawCurves;

        public SO_SplinePath.Unit CreateUnit(bool isPlayer) 
        {
            var unit = new SO_SplinePath.Unit()
            {
                isPlayer = isPlayer
            };

            return unit;
        }


        #region Inspector

        public void OnSceneDraw()
        {
            foreach (var i in Instances)
                i.OnSceneDraw_Nested();

            if (pegi.Handle.Button(transform.position, offset: (Vector3.up + Vector3.right) * 1.2f, label: DrawCurves ? "Hide Curves" : "Show Curves"))
                DrawCurves = !DrawCurves;
        }

        public void OnDrawGizmos() => this.OnSceneDraw_Nested();


        private readonly pegi.CollectionInspectorMeta _pathMeta = new("Paths", allowDeleting: false, showAddButton: false, showEditListButton: false);

        public override void Inspect()
        {
            "Curves".PegiLabel(60).ToggleIcon(ref DrawCurves).Nl();
            _pathMeta.Edit_List(Instances).Nl();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_SplinePath))] internal class PulseCommanderServiceDrawer : PEGI_Inspector_Override { }
}
