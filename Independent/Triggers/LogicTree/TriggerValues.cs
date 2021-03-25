using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers
{
    [Serializable]
    public class TriggerValues : IPEGI, IGotCount
    {
        public int Version = -1;

        [SerializeField] internal DictionaryOfTriggerGroups Groups = new();
       
        internal int this[IIntTriggerIndex index] 
        {
            get => index.IsValid() ? (Groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) ? vals[index] : 0) : 0;
            set 
            {
                var dic = Groups.GetOrCreate(index.GetGroupId());
                if (dic[index] != value)
                {
                    dic[index] = value;
                    Version++;
                }
            }
        }

        internal bool this[IBoolTriggerIndex index]
        {
            get => index.IsValid() && (Groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) && vals[index]);
            
            set
            {
                var dic = Groups.GetOrCreate(index.GetGroupId());
                if ((dic[index]) != value)
                {
                    dic[index] = value;
                    Version++;
                }
            }
        }

        public void Clear()
        {
            Groups.Clear();
        }

        #region Inspector

        public int GetCount() => Groups.Count;// + enumTags.CountForInspector + boolTags.CountForInspector; 

        public virtual void Inspect() 
        {
            pegi.nl();
            Groups.Nested_Inspect();
        }

        #endregion

 
        [Serializable]
        public class GroupOfTriggers : IPEGI, IGotCount, IPEGI_ListInspect
        {
            [SerializeField] internal TriggerValuesDictionary booleans = new();
            [SerializeField] internal TriggerValuesDictionary ints = new();

            internal int this[IIntTriggerIndex index]
            {
                get => ints[index];
                set => ints[index] = value;
            }

            internal bool this[IBoolTriggerIndex index]
            {
                get => booleans[index] > 0;
                set => booleans[index] = value ? 1 : 0;
            }

            public void Clear()
            {
                booleans.Clear();
                ints.Clear();

            }

            #region Inspector
            public int GetCount() => booleans.GetCount() + ints.GetCount();

            private int _inspectedStuff = -1;
            public void Inspect()
            {
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

            [Serializable]
            internal class TriggerValuesDictionary : IPEGI, IGotCount
            {
                [SerializeField] public DictionaryOfTriggerValues dictionary = new();

                [Serializable] public class DictionaryOfTriggerValues : SerializableDictionary<string, Trigger> { }

                internal int this[ITriggerIndex index]
                {
                    get => dictionary.TryGetValue(index.GetTriggerId(), out Trigger val) ? val.value : 0;
                    set
                    {
                        if (value == 0)
                            dictionary.Remove(index.GetTriggerId());
                        else
                            dictionary[index.GetTriggerId()] = new Trigger { value = value };
                    }
                }

                public void Clear() => dictionary.Clear();

                public int GetCount() => dictionary.Count;

                public void Inspect()
                {
                    dictionary.Nested_Inspect();
                }


                [Serializable]
                public struct Trigger : IPEGI_ListInspect
                {
                    [SerializeField] public int value;

                    public void InspectInList(ref int edited, int index)
                    {
                        pegi.edit(ref value);
                    }
                }

            }
        }

        [Serializable] 
        internal class DictionaryOfTriggerGroups : SerializableDictionary<string, GroupOfTriggers> { }
    }
}