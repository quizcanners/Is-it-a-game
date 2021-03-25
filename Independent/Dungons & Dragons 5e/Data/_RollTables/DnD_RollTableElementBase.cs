using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [Serializable]
    public abstract class RollTableElementWithSubTablesBase : RollTableElementBase, IPEGI
    {
        [SerializeField] protected List<RandomElementsRollTables> subTables = new();

        public BigTextEditable Description;

        public void UpdatePrototypes() 
        {
            foreach(var t in subTables) 
            {
                if (t)
                    t.UpdatePrototypes();
            }
        }

        public bool TryGetFallbackConcept<CT>(out CT value, RanDndSeed seed, IConceptValueProvider provider)
        {
            foreach (var t in subTables) 
            {
                if (t && t.TryGetConcept(out value, seed, provider))
                    return true;
            }

            value = default;
            return false;
        }

        public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) 
            where CT : IComparable => result.TryGetConcept(out value, subTables);

        internal void TrySelectInspect(List<RolledTable.Result> results)
        {
            if (subTables.Count < 3)
            {
                using (pegi.Indent(2))
                {
                    for (int i = 0; i < subTables.Count; i++)
                    {
                        var t = subTables[i];
                        if (t)
                        {
                            pegi.nl();
                            t.SelectInternal(results.GetOrCreate(i));
                        }
                    }
                }
            }
        }

        public virtual string GetRolledElementName(List<RolledTable.Result> subTableResults, bool shortText) => Description.GetRolledElementName(subTableResults, subTables, shortText);

        public void Roll(List<RolledTable.Result> results)
        {
            results.Clear();
            foreach (var sub in subTables)
            {
                var newRes = new RolledTable.Result();
                results.Add(newRes);

                if (sub)
                {
                    sub.Roll(newRes);
                }
            }
        }

        public override void Inspect(List<RolledTable.Result> subTablesRolls)
        {
            if (inspectedSubTable >= subTables.Count)
                inspectedSubTable = -1;

            if (subTables.Count == 1)
                inspectedSubTable = 0;

            if (inspectedSubTable == -1)
            {
                for (int i = 0; i < subTables.Count; i++)
                {
                    var t = subTables[i];
                    var r = subTablesRolls.GetOrCreate(i);
                    (icon.Enter.Click() | t.GetRolledElementName(r, shortText: true).PegiLabel().ClickLabel().nl()).OnChanged(() => inspectedSubTable = i);
                }
            }
            else
            {
                if (subTables.Count > 1 && icon.Exit.Click())
                    inspectedSubTable = -1;
                else
                {
                    subTables[inspectedSubTable].Inspect(subTablesRolls.GetOrCreate(inspectedSubTable));
                }
            }
        }

        [NonSerialized] private bool _addTable;

        public override void InspectInList(ref int edited, int ind)
        {
            base.InspectInList(ref edited, ind);

            if (subTables.Count == 0 && !_addTable)
            {
                if (icon.Condition.Click("Add SubTables"))
                    _addTable = true;
            }
            else
            {
                if (subTables.Count < 2)
                {
                    RandomElementsRollTables tmp = subTables.TryGet(0);
                    if (pegi.edit(ref tmp, 90))
                        subTables.ForceSet(0, tmp);
                }
                else
                    "X {0}".F(subTables.Count).PegiLabel(40).write();
            }
        }


        protected int inspectedSubTable = -1;

        public virtual void Inspect()
        {
            "Sub Table".PegiLabel().edit_List_UObj(subTables, ref inspectedSubTable);
        }
    }


    [Serializable]
    public abstract class RollTableElementBase : IPEGI_ListInspect, ICfgDecode, IGotReadOnlyName
    {

        public int Chances = 1;
        [NonSerialized] protected RollResult rangeStart;

        protected DnDPrototypesScriptableObject Data => Singleton.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);

        public abstract string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText);


        private static readonly char[] SPLITBY = new char[] { '-', '–' };

        public virtual void DecodeTag(string key, CfgData data)
        {
            if (key == "Roll" || (key.Length > 0 && key[0] == 'd'))
            {
                var val = data.ToString();

                bool isRange = false;
                foreach (var x in SPLITBY)
                {
                    if (val.Contains(x.ToString()))
                    {
                        isRange = true;
                        break;
                    }
                }

                if (isRange)
                {
                    var parts = val.Split(SPLITBY, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        var from = new CfgData(parts[0]).ToInt();
                        var to = new CfgData(parts[1]).ToInt();

                        if (to == 0)
                            to = 100;


                        Chances = Math.Max(1, to - from + 1);
                    }
                }
                else
                    Chances = 1;

                return;
            }

            switch (key)
            {
                case "Chances": Chances = data.ToInt(); break;
            }
        }

        #region Inspector

        internal static int inspectedMinRoll;
        internal static List<int> inspectedProbabilities;

        public virtual void Inspect(List<RolledTable.Result> results) { }

        public void SetRangeStart(ref RollResult rangeStart)
        {
            this.rangeStart = rangeStart;
            //_rangeSet = true;
            rangeStart += Chances;
        }

        public virtual string GetReadOnlyName() => ""; // _rangeSet ? "{0}-{1}".F(rangeStart, rangeStart + Chances - 1) : "";
        
        public virtual void InspectInList(ref int edited, int ind)
        {
            string rangeString;

            int totalProbability = 0;
            if (inspectedProbabilities != null) 
            {
                for (int i = 0; i < Chances; i++)
                    totalProbability += inspectedProbabilities.TryGet(rangeStart.Value + i - inspectedMinRoll, defaultValue: 0);
            }

            if (Chances > 1)
                rangeString = "{0}-{1}".F(rangeStart, rangeStart + Chances - 1);
            else
                rangeString = (rangeStart).ToString();

            if (rangeString.PegiLabel(45).edit(ref Chances, valueWidth: 35))
                Chances = Mathf.Max(1, Chances);

            if (totalProbability > 0)
                " ({0}%)".F(totalProbability).PegiLabel(35).write();
        }

        #endregion

      /*  [Serializable]
        public class BigTextEditable : IPEGI
        {
            public string Text;
            [SerializeField] private bool _editing;

            public void Inspect()
            {
                pegi.nl();
                if (_editing)
                {
                    pegi.editBig(ref Text);
                    if (icon.Done.Click())
                        _editing = false;
                }
                else
                {
                    if (Text.IsNullOrEmpty())
                    {
                        if ("Add Description".PegiLabel().Click())
                            _editing = true;
                    }
                    else
                    {
                        Text.write(pegi.Styles.OverflowText);

                        if (icon.Edit.Click().nl())
                            _editing = true;
                    }
                }
                pegi.nl();
            }
        }*/
    }
}