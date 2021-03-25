using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{

    [Serializable]
    public class BigTextEditable : IPEGI
    {
        public string Value;
        [NonSerialized] private string _tmpText;

        [NonSerialized] public bool Editing;
       

        public void Inspect()
        {
            if (Editing)
            {
                pegi.editBig(ref _tmpText);
                icon.Close.Click(() => Editing = false);
                icon.Save.Click(()=>
                {
                    Value = _tmpText;
                    Editing = false;
                });

                "<- Click when you are done".PegiLabel().writeHint();
                pegi.nl();
            }
            else
            {
                if (Value.IsNullOrEmpty())
                {
                    "Add Description".PegiLabel().Click(StartEdit).nl();
                }
                else
                {
                    icon.Edit.Click(StartEdit);
                }
                void StartEdit() 
                {
                    _tmpText = Value;
                    Editing = true;
                }
            }
        }

        
        public string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, List<RandomElementsRollTables> tables, bool shortText) 
        {
            var arguments = new List<string>();

            for (int i = 0; i < tables.Count; i++)
            {
                var t = tables[i];

                if (!t)
                {
                    arguments.Add("NULL TABLE");
                    continue;
                }

                arguments.Add(t.GetRolledElementName(seed, provider, shortText));
            }

            if (!Value.IsNullOrEmpty())
            {
                try
                {
                    var res = string.Format(Value, arguments.ToArray());
                    return res;
                }
                catch
                {

                }
            }

            var sb = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                var t = arguments[i];
                if (sb.Length > 0)
                    sb.Append(' ');

                sb.Append(t);
            }

            return sb.ToString();
        }

        public string GetRolledElementName(List<RolledTable.Result> results, List<RandomElementsRollTables> tables, bool shortText)
        {
            var arguments = new List<string>();

            for (int i = 0; i < tables.Count; i++)
            {
                var t = tables[i];
                var r = results.TryGet(i);

                if (!t) 
                {
                    arguments.Add("NULL TABLE");
                    continue;
                }

                if (r == null)
                {
                    arguments.Add(" Not rolled");
                    break;
                }
                arguments.Add(t.GetRolledElementName(r, shortText));
            }

            if (!Value.IsNullOrEmpty())
            {
                try
                {
                    var res = string.Format(Value, arguments.ToArray());
                    return res;
                }
                catch 
                {
                    
                }
            }

            if (arguments.IsNullOrEmpty())
                return "";

            var sb = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                var t = arguments[i];
                if (sb.Length > 0)
                    sb.Append(' ');

                sb.Append(t);
            }

            return sb.ToString();
        }
    }
}