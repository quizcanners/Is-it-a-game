using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers
{

#pragma warning disable IDE0018 // Inline variable declaration


    public class ConditionBranch : IPEGI, IAmConditional, ICanBeDefaultCfg, IPEGI_ListInspect, IGotCount, ISearchable, IGotName, INeedAttention {

         private string _key = "";
         private ConditionBranchType _type = ConditionBranchType.And;
         private List<ConditionBase> _conditions = new();
         private List<ConditionBranch> _branches = new();

        #region Inspector

        public int GetCount() => CountRecursive();

        private int CountRecursive()
        {
            var count = _conditions.Count;

            foreach (var b in _branches)
                count += b.CountRecursive();

            return count;
        }

        [SerializeField] private int _browsedBranch = -1;
        [SerializeField] private int _browsedCondition = -1;

        public string NeedAttention()
        {
            string msg;
            if (pegi.NeedsAttention(_branches, out msg) || pegi.NeedsAttention(_conditions, out msg))
                return msg;

            return null;
        }
        
        public string NameForInspector { get { return _key; } set { _key = value; } }

        private readonly LoopLock _searchLoopLock = new();

        public IEnumerator<object> SearchKeywordsEnumerator()
        {
            if (!_searchLoopLock.Unlocked) 
                yield break;

            using (_searchLoopLock.Lock())
            {
                foreach (var c in _conditions)
                    yield return c;

                foreach (var b in _branches)
                    yield return b;
            }
        }

        public virtual void Inspect()
        {
            if (!_key.IsNullOrEmpty())
                _key.PegiLabel(style: pegi.Styles.ListLabel).nl();

            if (_browsedBranch == -1)
            {
                var cnt = GetCount();

                if (cnt > 1)
                {
                    if (_type.ToString().PegiLabel((_type == ConditionBranchType.And
                        ? "All conditions and sub branches should be true"
                        : "At least one condition OR sub branch should be true")).Click())
                        _type = (_type == ConditionBranchType.And ? ConditionBranchType.Or : ConditionBranchType.And);
                }

                if (cnt>0)
                    (CheckConditions() ? icon.Active : icon.InActive).nl();

                "Conditions".PegiLabel().edit_List(_conditions, ref _browsedCondition);
            }
            else 
                _browsedCondition = -1;
            

            if (_browsedCondition == -1)
            {
                pegi.line(Color.black);

                if (_branches.Count == 0 && "Create Sub Branch".PegiLabel().Click().nl())
                    _branches.Add(new ConditionBranch());

                if (_branches.Count > 0)
                    "Sub Branches".PegiLabel().edit_List(_branches, ref _browsedBranch);
            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            //if ((IsTrue ? icon.Active : icon.InActive).Click() && !TryForceTo(TriggerValues.Global, !IsTrue))
              //  Debug.Log("No Conditions to force to {0}".F(!IsTrue));

            var cnt = GetCount();

            switch (cnt)
            {
                case 0:
                    "{0}: Unconditional".F(_key).PegiLabel().write();
                    break;
                case 1:
                    if (_conditions.Count == 1)
                        "{0}: {1}".F(_key, _conditions[0].GetReadOnlyName()).PegiLabel().write();
                    else goto default;
                    break;
                default:
                    if (_branches.Count>0)
                        "{0}: {1} conditions; {2} branches".F(_key, cnt, _branches.Count).PegiLabel().write();
                    else 
                        "{0}: {1} conditions".F(_key, cnt).PegiLabel().write();
                    break;
            }

            if (this.Click_Enter_Attention(icon.Enter, "Explore Condition branch", false))
                edited = ind;
        }
       
        #endregion
        
        #region Encode & Decode
        public virtual bool IsDefault => (_conditions.Count == 0 && _branches.Count == 0);

        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_IfNotEmpty("wb",         _branches)
            .Add_IfNotEmpty_Derrived("v",          _conditions)
            .Add("t",                     (int)_type)
            .Add_IfNotNegative("insB",    _browsedBranch)
            .Add_IfNotNegative("ic",      _browsedCondition);
        


        public virtual void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "t":     _type = (ConditionBranchType)data.ToInt(); break;
                case "wb":    data.ToList(out _branches); break;
                case "v":     data.ToList_Derrived(out _conditions); break;
                case "insB":  _browsedBranch = data.ToInt(); break;
                case "ic":    _browsedCondition = data.ToInt(); break;
               
            }
        }
        #endregion

        public bool CheckConditions() {

            switch (_type) {
                case ConditionBranchType.And:
                    foreach (var c in _conditions)
                        if (!c.IsConditionTrue()) return false;
                    foreach (var b in _branches)
                        if (!b.CheckConditions()) return false;
                    return true;
                case ConditionBranchType.Or:
                    foreach (var c in _conditions)
                        if (c.IsConditionTrue())
                            return true;
                    foreach (var b in _branches)
                        if (b.CheckConditions()) return true;
                    return (_conditions.Count == 0 && _branches.Count == 0);
            }
            return true;
        }

        public bool TryForceTo(bool toTrue)  {

            if ((toTrue && _type == ConditionBranchType.And) || (!toTrue && _type == ConditionBranchType.Or)) {
                var anyApplied = false;
                foreach (var c in _conditions)
                    anyApplied |= c.TryForceConditionValue(toTrue);
                foreach (var b in _branches)
                    anyApplied |= b.TryForceTo(toTrue);

                return toTrue || anyApplied;
            }

            foreach (var c in _conditions)
                if (c.TryForceConditionValue(toTrue))
                    return true;
                
            foreach (var b in _branches)
                if (b.TryForceTo(toTrue))
                    return true;

            return toTrue;

        }

     

        public ConditionBranch() { }

        public ConditionBranch(string usage)
        {
            _key = usage;
        }

        private enum ConditionBranchType { Or, And }
    }
}

