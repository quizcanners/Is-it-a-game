using System;
using System.Collections.Generic;
using System.Text;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;


namespace Dungeons_and_Dragons.Tables
{
    
    [CreateAssetMenu(fileName = FILE_NAME,  menuName = TABLE_CREATE_NEW_PATH + FILE_NAME)]
    public class RollTable : RollTableGeneric<RollTable.Element>, INeedAttention
    {
        public const string FILE_NAME = "Generic";

        public List<Element> elements = new();

        private Element this[RolledTable.Result value] => Get(elements, value);
        private Element this[RanDndSeed seed] => Get(elements, RollDices(seed));

        protected override void RollInternal(RolledTable.Result result) 
        {
            result.Roll = RollDices();
            var el = Get(elements, result.Roll);
            if (el != null)
                el.Roll(result.SubResultsList);
        }

        public override void Inspect(RolledTable.Result result)
        {
            icon.Dice.Click(()=> Roll(result));

            Element el = Get(elements, result.Roll);

            SelectInternal(result);

            this.ClickHighlight().nl();

            if (el != null)// && icon.Enter.isEntered(ref _inspectingRollResult).nl())
                el.Inspect(result.SubResultsList);
            
        }

        internal override void SelectInternal(RolledTable.Result result)
        {
            Element el = Get(elements, result.Roll);
            if (pegi.select(ref el,  elements, stripSlashes: true) && el != null)
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }

            if (el != null)
                el.TrySelectInspect(result.SubResultsList);
        }

        private int _editedElement = -1;

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() | "{0} | {1}".F(index, name.Replace("Random", "")).PegiLabel(width: 120).ClickLabel())
                edited = index;
        }

        protected override List<Element> List { get => elements; set => elements = value; }

        protected override bool EditList() =>
            "{0} {1}".F(_dicesToRoll.ToRollTableDescription(), QcSharp.KeyToReadablaString(name.SimplifyTypeName())).PegiLabel().edit_List(elements, ref _editedElement).nl();

        public override string GetRolledElementName(RolledTable.Result result, bool shortText)
        {
            if (!result.IsRolled)
                return "Not Rolled";

            var el = this[result];
            if (el == null)
                return "NULL for {0}".F(result.Roll);

            return el.GetRolledElementName(result.SubResultsList, shortText);
        }

        public override string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText)
        {
            var el = this[seed];

            if (el == null)
                return "NULL";

            return el.GetRolledElementName(seed, provider, shortText);
        }

        public override bool TryGetConcept<CT>(out CT value, RolledTable.Result result)
        {
            var el = this[result];

            if (el != null)
            {
                return el.TryGetConcept(out value, result.SubResult);
            }

            return base.TryGetConcept(out value, result);
        }

        public override void UpdatePrototypes()
        {
            base.UpdatePrototypes();

            foreach (var el in elements)
                el.UpdatePrototypes();
        }

        public string NeedAttention()
        {
            var tot = GetTotalRange();
            if (tot != _dicesToRoll.MaxRoll())
                return "Total Range is {0} != {1}".F(tot, _dicesToRoll.MaxRoll().Value);

                return null;
        }

      
        [Serializable]
        public class Element : RollTableElementWithSubTablesBase, IGotName
        {
            public string Name;
            [SerializeField] private bool _includeName = true;
            public string NameForInspector { get => Name; set => Name = value; }

            public override string GetReadOnlyName() =>  "{0} {1}".F(base.GetReadOnlyName(), Name) ;

            public override string GetRolledElementName(List<RolledTable.Result> subTableResults, bool shortText)
            {
                var sub = base.GetRolledElementName(subTableResults, shortText);
                if (!sub.IsNullOrEmpty())
                    return _includeName ? "{0} {1}".F(Name, sub) : sub;

               return Name;
            }

            public override string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText) 
            {
                if (subTables.Count == 1) 
                {
                    var t = subTables[0];
                    if (t)
                        return t.GetRolledElementName(seed, provider, shortText);
                }

                return Name;
            }

            public override void DecodeTag(string key, CfgData data)
            {
                switch (key) 
                {
                    case "Name":
                    case "name":
                        Name = data.ToString();
                        break;
                    case "Description": Description.Value = data.ToString(); break;
                    default:   base.DecodeTag(key, data); break;
                }
            }

            public override void Inspect(List<RolledTable.Result> subTablesRolls) 
            {
                if (inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);

                base.Inspect();
            }

            public override void InspectInList(ref int edited, int ind)
            {
                base.InspectInList(ref edited, ind);

                if (subTables.Count == 1 && subTables[0])
                {
                    //var newName = subTables[0].name.Replace("Random ", "");
                    //if (!newName.Equals(Name)) 
                       // "->".PegiLabel().Click(toolTip: "Table Name to Element Name", onClick: () => Name = newName);
                }

                pegi.edit(ref Name);
              
                if (icon.Enter.Click())
                    edited = ind;
            }

            public override void Inspect()
            {
                if (inspectedSubTable == -1)
                {
                    Description.Nested_Inspect(fromNewLine: false);
                    if (subTables.Count>0)
                        "Include Name".PegiLabel().toggleIcon(ref _includeName).nl();

                    if (!Description.Value.IsNullOrEmpty())
                        Description.Value.PegiLabel().writeHint();

                    pegi.nl();
                }
                base.Inspect();
            }

         
        }
    }

    [PEGI_Inspector_Override(typeof(RollTable))] internal class RollTableDrawer : PEGI_Inspector_Override { }
}