using System;
using System.Collections;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuizCanners.IsItGame
{
    public abstract class EnumeratedAssetListsBase<T, G> : ScriptableObject, IPEGI where T : struct, IComparable, IFormattable, IConvertible where G : Object
    {

       // public Object defaultAsset;

        [SerializeField] protected List<EnumeratedObjectList> enumeratedObjects = new List<EnumeratedObjectList>();

        private bool TryGet(T value, out EnumeratedObjectList obj)
        {
            int index = Convert.ToInt32(value);

            if (enumeratedObjects.Count > index)
            {
                obj = enumeratedObjects[index];
                return true;
            }

            obj = null;

            return false;
        }

        public virtual G Get(T enumKey) => (TryGet(enumKey, out EnumeratedObjectList sp) ? sp.list.GetRandom() : null) as G;
        

        #region Inspector

        private int _inspectedList = -1;

        public virtual void Inspect()
        {

           // "Defaul {0}".F(typeof(G).ToPegiStringType()).edit(120, ref defaultAsset, allowSceneObjects: true).nl();

            EnumeratedObjectList.inspectedEnum = typeof(T);
            EnumeratedObjectList.inspectedObjectType = typeof(G);

            "Enumerated {0}".F(typeof(G).ToPegiStringType()).PegiLabel().edit_List(enumeratedObjects, ref _inspectedList).nl();

        }
        #endregion



    }

    [Serializable]
    public class EnumeratedObjectList : IPEGI_ListInspect, IGotReadOnlyName, IPEGI, IGotCount
    {
        [SerializeField] private string nameForInspector = "";
        public List<Object> list;

        #region Inspector
        public static Type inspectedEnum;
        public static Type inspectedObjectType;

        public void InspectInList(ref int edited, int ind)
        {
            var changeToken = pegi.ChangeTrackStart();

            var name = Enum.ToObject(inspectedEnum, ind).ToString();

            if (!nameForInspector.Equals(name))
            {
                nameForInspector = name;
                changeToken.Changed = true;
            }

            "{0} [{1}]".F(nameForInspector, GetCount()).PegiLabel().write();

            if (list == null)
            {
                list = new List<Object>();
                changeToken.Changed = true;
            }

            if (list.Count < 2)
            {
                var el = list.TryGet(0);

                if (pegi.edit(ref el, inspectedObjectType, 90))
                    list.ForceSet(0, el);
            }

            if (icon.Enter.Click())
                edited = ind;
        }

        public int GetCount() => list.IsNullOrEmpty() ? 0 : list.Count;

        public string GetReadOnlyName() => nameForInspector + " " + (list.IsNullOrEmpty() ? "Empty" : pegi.GetNameForInspector(list[0]));

        public void Inspect()
        {
            "All {0}".F(nameForInspector).PegiLabel().edit_List_UObj(list);
        }



        #endregion
    }
}

