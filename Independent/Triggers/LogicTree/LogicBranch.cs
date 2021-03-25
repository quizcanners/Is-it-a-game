using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers
{

    public class LogicBranch<T> : IGotName , IPEGI, IAmConditional, ICanBeDefaultCfg, ISearchable, IGotCount  where T: class, ICfg, new() {

        public string name = "no name";

        public List<LogicBranch<T>> subBranches = new();

        public ConditionBranch conditions = new();

        public List<T> elements = new();

        public virtual bool IsDefault => subBranches.Count ==0 && conditions.IsDefault && elements.Count == 0;

        public List<T> CollectAll(ref List<T> lst) {

            lst.AddRange(elements);

            foreach (var b in subBranches)
                b.CollectAll(ref lst);

            return lst;
        }

        public int GetCount()
        {
            int count = 0;
            GetCountInternal(ref count);
            return count;
        }

        private readonly LoopLock _debugLock = new();

        private void GetCountInternal(ref int count)
        {
            if (_debugLock.Unlocked)
            {
                using (_debugLock.Lock())
                {
                    foreach (var b in subBranches)
                    {
                        count++;
                        b.GetCountInternal(ref count);
                    }
                }
            } else
            {
                Debug.LogError("Stack overflow in Logic Branch");
            }
        }


        public bool CheckConditions() => conditions.CheckConditions();

        #region Encode & Decode



        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("name", name)
            .Add("cond", conditions)
            .Add_IfNotEmpty("sub", subBranches)
            .Add_IfNotEmpty("el", elements)
            .Add_IfNotNegative("ie", _inspectedElement)
            .Add_IfNotNegative("br", _inspectedBranch);
        
        public virtual void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "name": name = data.ToString(); break;
                case "cond": conditions.Decode(data); break;
                case "sub": data.ToList(out subBranches); break;
                case "el": data.ToList(out elements); break;
                case "ie": _inspectedElement = data.ToInt(); break;
                case "br": _inspectedBranch = data.ToInt(); break;
            }
        }
        #endregion

        #region Inspector

        public virtual string NameForElements => typeof(T).ToPegiStringType();

        public string NameForInspector
        {
            get { return name; }
            set { name = value; }
        }

        public void ResetInspector() {
            _inspectedElement = -1;
            _inspectedBranch = -1;
           //base.ResetInspector();
        }

        private int _inspectedElement = -1;
        private int _inspectedBranch = -1;
        [SerializeField] private pegi.EnterExitContext context = new(); // = -1;

        private readonly LoopLock searchLoopLock = new();

      

        private static LogicBranch<T> parent;

        public virtual void Inspect() {

            using (context.StartContext())
            {

                pegi.nl();

                if (parent != null || conditions.GetCount() > 0)
                    conditions.enter_Inspect_AsList().nl();

                parent = this;

                NameForElements.PegiLabel().enter_List(elements, ref _inspectedElement).nl();

                (NameForInspector + "=>Branches").PegiLabel().enter_List(subBranches, ref _inspectedBranch).nl();
            }

            parent = null;
        }

        public IEnumerator<object> SearchKeywordsEnumerator()
        {
            if (searchLoopLock.Unlocked)
                using (searchLoopLock.Lock())
                {
                    yield return conditions;

                    foreach (var e in elements)
                        yield return e;

                    foreach (var sb in subBranches)
                        yield return sb;
                }
        }



        #endregion

    }
}