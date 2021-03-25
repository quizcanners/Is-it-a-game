using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers
{

    [CreateAssetMenu(fileName = FILE_NAME+ ".triggers", menuName = "Quiz Canners/" + TriggerValuesService.TRIGGERS + "/" + FILE_NAME)]
    public class TriggerGroupMeta : ScriptableObject, IPEGI, IGotName
    {
        public const string FILE_NAME = "Triggers Meta";

        [SerializeField] internal TriggerDictionary booleans = new();
        [SerializeField] internal TriggerDictionary ints = new();

        public string NameForInspector { get => name; set => QcUnity.RenameAsset(this, value); }

        public void Clear()
        {
            booleans.Clear();
            ints.Clear();
        }

        public int GetCount() => booleans.Count + ints.Count;

        internal TriggerMeta this[IIntTriggerIndex index]
        {
            get => ints.GetOrCreate(index.GetTriggerId());
            set => ints[index.GetTriggerId()] = value;
        }

        internal TriggerMeta this[IBoolTriggerIndex index]
        {
            get => booleans.GetOrCreate(index.GetTriggerId());
            set => booleans[index.GetTriggerId()] = value;
        }

        internal TriggerDictionary GetDictionary(ITriggerIndex index) => index.IsBooleanValue ? booleans : ints;
        
        #region Inspector

        private int _inspectedStuff = -1;
        public void Inspect()
        {
            pegi.nl();
            "Booleans".PegiLabel().enter_Inspect(booleans, ref _inspectedStuff, 0).nl();
            "Integers".PegiLabel().enter_Inspect(ints, ref _inspectedStuff, 1).nl();
        }

        public void InspectInList(ref int edited, int index)
        {
            if (icon.Clear.ClickConfirm(confirmationTag: "Erase " + index, "Erase all valus from the group?"))
                Clear();

            if (icon.Enter.Click())
                edited = index;
        }
        #endregion

        [Serializable] internal class TriggerDictionary : SerializableDictionary<string, TriggerMeta> { }

        [Serializable]
        public class TriggerMeta : IPEGI_ListInspect
        {
            [SerializeField] private string hint = "";

            public void InspectInList(ref int edited, int index)
            {
                pegi.edit(ref hint);
            }
        }
    }

    [PEGI_Inspector_Override(typeof(TriggerGroupMeta))] internal class TriggerValuesKeyCollectionDrawer : PEGI_Inspector_Override { }
}