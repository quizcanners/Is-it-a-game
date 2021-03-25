using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuizCanners.IsItGame
{

    public abstract class EnumeratedAssetBase<T, G> : ScriptableObject, IPEGI where T : struct, IComparable, IFormattable, IConvertible where G : Object
    {

        public Object defaultAsset;

        [SerializeField] protected List<EnumeratedObject> enumeratedObjects = new List<EnumeratedObject>();

        private bool TryGet(T value, out EnumeratedObject obj)
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

        public virtual G Get(T enumKey)
        {

            return (TryGet(enumKey, out EnumeratedObject sp) ? sp.value : defaultAsset) as G;
        }

        #region Inspector
        public virtual void Inspect()
        {

            "Defaul {0}".F(typeof(G).ToPegiStringType()).PegiLabel(120).edit(ref defaultAsset, allowSceneObjects: true).nl();

            EnumeratedObject.inspectedEnum = typeof(T);
            EnumeratedObject.inspectedObjectType = typeof(G);

            "Enumerated {0}".F(typeof(G).ToPegiStringType()).PegiLabel().edit_List(enumeratedObjects).nl();
        }
        #endregion
    }

    [Serializable]
    public class EnumeratedObject : IPEGI_ListInspect, IGotReadOnlyName
    {
        [SerializeField] private string nameForInspector = "";
        public Object value;

        #region Inspector
        public static Type inspectedEnum;
        public static Type inspectedObjectType;

        public void InspectInList(ref int edited, int ind)
        {

            var name = Enum.ToObject(inspectedEnum, ind).ToString();

            if (!nameForInspector.Equals(name))
            {
                nameForInspector = name;
            }

            nameForInspector.PegiLabel(90).write();

            pegi.edit(ref value, inspectedObjectType);
        }

        public string GetReadOnlyName() => nameForInspector + " " + (value ? value.name : ("No " + inspectedObjectType));

        #endregion
    }

}