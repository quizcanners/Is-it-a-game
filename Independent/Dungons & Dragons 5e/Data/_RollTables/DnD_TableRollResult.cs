using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [Serializable]
    public class TableRollResult : IPEGI, IGotReadOnlyName, IGotName, IPEGI_ListInspect, ISerializationCallbackReceiver, ICfg
    {
        [SerializeField] private string _name;
        [SerializeField] private string tableKey;

        public RolledTable.Result result = new();

        private RandomElementsRollTablesDictionary Tables => Singleton.TryGetValue<DnD_Service, RandomElementsRollTablesDictionary>(s => s.DnDPrototypes.RollTables);

        public void Roll()
        {
            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                    table.Roll(result);
                else
                    Debug.LogError("Table not found");
            }
        }


        private IDisposable TryGetTableDisposable(out RandomElementsRollTables table, out bool tableFound) 
        {
            tableFound = false;

            if (tableKey.IsNullOrEmpty() || Tables == null) 
            {
                table = null;
                return null;
            }

            tableFound = Tables.TryGetValue(tableKey, out table) && table;

            if (tableFound) 
            {
                return result.AddAndUse(table);
            }
            return null;
        }

        #region Encode & Decode

        public CfgEncoder Encode() => new CfgEncoder()
            .Add_String("n", _name)
            .Add_String("t", tableKey)
            .Add("r", result);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                case "n": _name = data.ToString(); break;
                case "t": tableKey = data.ToString(); break;
                case "r": result.Decode(data); break;
            }
        }

        [SerializeField] private CfgData _data;
        public void OnBeforeSerialize() =>  _data = result.Encode().CfgData;
        public void OnAfterDeserialize()
        {
            result = new RolledTable.Result();
            _data.DecodeOverride(ref result);
        }

        #endregion

        #region Inspector
        public string NameForInspector { get => _name; set => _name = value; }
        public string GetReadOnlyName() => tableKey;

        public void Inspect()
        {
            "Table".PegiLabel(70).select(ref tableKey, Tables);
            icon.Save.Click(OnBeforeSerialize);
            icon.Load.Click(OnAfterDeserialize);

         

            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                {
                    icon.Dice.Click(Roll);
                    table.ClickHighlight().nl();
                    table.Inspect(result);
                }
            }
        }

        public void InspectInList(ref int edited, int index)
        {
            pegi.edit(ref _name);

            pegi.select(ref tableKey, Tables);

            if (icon.Enter.Click())
                edited = index;

            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                {
                    table.ClickHighlight();

                    icon.Dice.Click(() => Roll());

                    using (pegi.Indent())
                    {
                        pegi.nl();
                        table.GetRolledElementName(result, shortText: true).PegiLabel(pegi.Styles.HintText).write();
                    }
                }
            }
        }

        #endregion
    }
}