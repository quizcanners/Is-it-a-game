using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    public class DnD_RollTable_ConceptConditionGeneric<T> : RandomElementsRollTables, IPEGI where T: IComparable
    {
        [SerializeField] private List<Element> _elements = new();
        [SerializeField] private DefaultConcept _defaultOne = DefaultConcept.First;

        private int _inspectedElement = -1;

        private enum DefaultConcept 
        { 
            Random = 0, First = 1 
        }

        public override bool TryGetConcept<CT>(out CT value, RanDndSeed seed, IConceptValueProvider provider)
        {
            if (TryGetByConcept(provider, out var el, _defaultOne) && el.Table)
            {
                if (el.Table.TryGetConcept(out value, seed, provider))
                    return true;
            }

            return base.TryGetConcept(out value, seed, provider);
        }

        public override string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText)
        {
            if (TryGetByConcept(provider, out Element el, DefaultConcept.First))
            {
                return el.GetRolledElementName(seed, provider, shortText: shortText);
            }

            return "";
        }

        public override string GetRolledElementName(RolledTable.Result result, bool shortText)
        {
            if (!result.IsRolled) 
            {
                return "Not Rolled";
            }

            if (!TryGetByConcept(result, out var el, _defaultOne))
                return "El {0} not found".F((T)(object)result.Roll.Value);

            return el.GetRolledElementName(result.SubResult, shortText);
        }

        protected override void RollInternal(RolledTable.Result result)
        {
            if (TryGetByConcept(result, out Element el, _defaultOne))
            {
                result.Roll = RollResult.From(el.Condition);
                el.Table.Roll(result.SubResult);
            }
        }

        public override bool TryGetConcept<CT>(out CT value, RolledTable.Result result)
        {
            if (typeof(CT) == typeof(T)) 
            {
                value = default;
                return false;
            }

            if (TryGetByConcept(result, out Element el, _defaultOne))
            {
                return el.TryGetConcept(out value, result.SubResult);
            }
                
            return base.TryGetConcept(out value, result);
        }

        private bool TryGetByConcept(IConceptValueProvider prov, out Element element, DefaultConcept defaultGetter)
        {
            if (prov != null)
            {
                if (prov.TryGetConcept<T>(out var ccpt))
                {
                    var ind = (int)(object)ccpt;

                    foreach (var el in _elements)
                    {
                        if (el.Condition == ind)
                        {
                            element = el;
                            return true;
                        }
                    }
                } 
            }

            switch (defaultGetter) 
            {
                case DefaultConcept.First: element = _elements.TryGet(0); break;
                case DefaultConcept.Random:
                default: 
                    element = _elements.GetRandom(); break;
            }
           
            return element != null;
        }


        #region Inspector
        public void Inspect()
        {
            pegi.nl();

            if (_elements.Count == 0 && "Auto-Fill".PegiLabel().Click().nl())
            {
                var enumValues = Enum.GetValues(typeof(T));

                foreach (var eVal in enumValues)
                    _elements.Add(new Element() { Condition = (int)eVal });
            }

            if (_inspectedElement == -1)
                "On Default".PegiLabel(80).editEnum(ref _defaultOne).nl();

            "Conditional Elements".PegiLabel().edit_List(_elements, ref _inspectedElement);
        }

        public override void Inspect(RolledTable.Result result)
        {
            TryGetByConcept(result, out var el, _defaultOne);

            if ("Sub Table".PegiLabel(90).select(ref el, _elements).nl())
            {
                result.Roll = RollResult.From(el.Condition);
                el.Roll(result.SubResult);
            }

            if (el != null)
                el.Inspect(result.SubResult);
        }
        #endregion

        [Serializable]
        public class Element : IPEGI, IPEGI_ListInspect, IGotReadOnlyName
        {
            public int Condition;
            public RandomElementsRollTables Table;

            public T ConditionEnum 
            {
                get => (T)(object)Condition;
                set => Condition = (int)(object)value;
            }

            public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT : IComparable 
            {
                if (Table)
                    return Table.TryGetConcept(out value, result);

                value = default;
                return false;
            }

            public string GetReadOnlyName() => ConditionEnum.ToString();

            public void Roll(RolledTable.Result result)
            {
                if (Table)
                    Table.Roll(result);
            }

            public string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText) 
            {
                if (!Table)
                    return "No Table";
                
                return Table.GetRolledElementName(seed, provider, shortText);
            }

            public string GetRolledElementName(RolledTable.Result result, bool shortText) 
            {
                if (!Table) 
                    return "No Table";
                
                return Table.GetRolledElementName(result, shortText); 
            }

            #region Inspector
            public void Inspect(RolledTable.Result result)
            {
                if (!Table)
                    pegi.edit(ref Table).nl();
                else
                    Table.Inspect(result);
            }

            public void InspectInList(ref int edited, int ind)
            {
                pegi.editEnum<T>(ref Condition);
                pegi.edit(ref Table);
                if (icon.Enter.Click())
                    edited = ind;
            }

            public void Inspect()
            {
                "Condition".PegiLabel(90).editEnum<T>(ref Condition).nl();
                pegi.edit(ref Table).nl();
                pegi.Try_Nested_Inspect(Table);
            }

            #endregion
        }
    }
}