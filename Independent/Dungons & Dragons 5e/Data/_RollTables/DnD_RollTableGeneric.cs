using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    public abstract class RollTableGeneric<T> : RandomElementsRollTables, IPEGI where T : RollTableElementBase, new()
    {
        [SerializeField] protected QcGoogleSheetToCfg _sheetParcer;

        public List<Dice> _dicesToRoll = new();

        protected abstract List<T> List { get; set; }
        protected RollResult RollDices() => _dicesToRoll.Roll();
        protected RollResult RollDices(RanDndSeed seed) => _dicesToRoll.Roll(seed, name.GetHashCode());
        protected T GetRandom() => Get(List, RollDices());
        protected RollResult GetTargetRoll(List<T> fromTable, T el)
        {
            var nextElementFirstIndex = _dicesToRoll.MinRoll();

            foreach (var item in fromTable)
            {
                if (item == el)
                    return nextElementFirstIndex;

                nextElementFirstIndex += item.Chances;
            }

            return new RollResult();
        }

        protected T Get(List<T> fromTable, RolledTable.Result result) => Get(fromTable, result.Roll);

        protected T Get(List<T> fromTable, RollResult rollResult)
        {
            var nextElementFirstIndex = _dicesToRoll.MinRoll();

            foreach (var item in fromTable)
            {
                if ((rollResult >= nextElementFirstIndex) && (rollResult < (nextElementFirstIndex + item.Chances)))
                    return item;

                nextElementFirstIndex += item.Chances;
            }

            return null;//fromTable.TryGet(0);
        }


        public override void UpdatePrototypes() 
        {
            if (_sheetParcer.IsDownloading() || !_sheetParcer.NeedAttention().IsNullOrEmpty())
                return;

            Singleton.Try<DnD_Service>(s => s.StartCoroutine(_sheetParcer.DownloadingCoro(
                      onFinished: () =>
                      {
                          var slt = List;
                         
                          var cols = _sheetParcer.Columns;

                          var zeroCol = cols[0];

                          if (zeroCol.IsNullOrEmpty() == false && zeroCol.Contains("d"))
                          {
                              var diceValue = zeroCol[1..];

                              if (int.TryParse(diceValue, out int dice))
                              {
                                  _dicesToRoll.Clear();
                                  _dicesToRoll.Add((Dice)dice);
                              }
                          }

                          bool foundName = false;

                          for (int i=0; i<cols.Count; i++) 
                          {
                              var col = cols[i];

                              if (col.Equals("Name") || col.Equals("name")) 
                              {
                                  foundName = true;
                                  break;
                              }
                          }

                          if (!foundName)
                          {
                              Debug.LogWarning("Name column wasn't found in {0}. Replacing {1} with Name".F(name, cols.TryGet(1)));
                              cols.ForceSet(1, "Name");
                          }

                          _sheetParcer.ToListOverride(ref slt);

                      })));
        }


        protected RollResult GetTotalRange() 
        {
            var rangeStart = _dicesToRoll.MinRoll();

            var lst = List;

            if (lst != null)
                for (int i = 0; i < lst.Count; i++)// var el in lst)
                {
                    var el = lst[i];

                    if (el != null)
                        el.SetRangeStart(ref rangeStart);
                }

            return rangeStart - 1; ;
        }

        #region Inspector
        protected int _inspectedStuff = -1;
        protected abstract bool EditList();

        public virtual void Inspect()
        {
            this.ClickHighlight();

            pegi.nl();

            var endOfRange = GetTotalRange();
            var lst = List;
            int groupIndex = -1;

            if ("Dices: {0}".F(_dicesToRoll.ToRollTableDescription(showPossibiliesNumber: true)).PegiLabel().isEntered(ref _inspectedStuff, ++groupIndex).nl())
            {
                "Table: {0} - elements, {1} - total Value".F(lst.Count, _dicesToRoll.MinRoll() - _dicesToRoll.MinRoll()).PegiLabel().writeHint();
                pegi.nl();

                if (_dicesToRoll == null)
                {
                    if ("Add Dice".PegiLabel().Click())
                        _dicesToRoll = new List<Dice>() { Dice.D20 };
                }
                else
                {
                    pegi.edit_List_Enum(_dicesToRoll, defaultValue: Dice.D20).nl();
                }
            }

            if ("Table ({0} elements)".F(List.Count).PegiLabel().isEntered(ref _inspectedStuff, ++groupIndex).nl())
            {
                if (_dicesToRoll.Count > 1)
                {
                    "Avarage Roll: {0}".F(_dicesToRoll.AvargeRoll()).PegiLabel().nl();
                    RollTableElementBase.inspectedProbabilities = _dicesToRoll.CalculateRollResultProbabilities();
                    RollTableElementBase.inspectedMinRoll = _dicesToRoll.MinRoll().Value;
                }
                else
                    RollTableElementBase.inspectedProbabilities = null;

                EditList();

                if (endOfRange != _dicesToRoll.MaxRoll())
                    "{0}/{1}".F(endOfRange, _dicesToRoll.MaxRoll()).PegiLabel().nl();
            }

            _sheetParcer.enter_Inspect_AsList(ref _inspectedStuff, ++groupIndex); 
            pegi.nl_ifEntered();

            if (_inspectedStuff == groupIndex)  //"Download from Google Sheet".PegiLabel().isEntered(ref _inspectedStuff, ++groupIndex).nl_ifEntered())
            {
                if (_sheetParcer.IsDownloading())
                    "Downloading...".PegiLabel().nl();
                else if (_sheetParcer.IsDownloaded && "Update Table".PegiLabel().Click())
                {
                    var slt = List;
                    _sheetParcer.ToListOverride(ref slt);
                }
            }

            if (_inspectedStuff == -1) 
            {
                if (_sheetParcer.IsDownloading())
                    icon.Wait.draw();
                else if (_sheetParcer.NeedAttention().IsNullOrEmpty())
                    icon.Download.Click(UpdatePrototypes);
            }


            pegi.nl();

            if (typeof(RollTableElementWithSubTablesBase).IsAssignableFrom(typeof(T)))
            {
                if ("Description".PegiLabel().isEntered(ref _inspectedStuff, ++groupIndex).nl())
                {
                    var el = List[0] as RollTableElementWithSubTablesBase;
                    if (el != null)
                    {
                        el.Description.Nested_Inspect().nl();
                    }

                    if ("Copy Description to other elements".PegiLabel().ClickConfirm(confirmationTag: "Will override whatever is there in other elements")) 
                    {
                        foreach(var e in List)
                        {
                            if (e is RollTableElementWithSubTablesBase sb)
                            {
                                sb.Description.Value = el.Description.Value;
                            }
                        }
                    }
                }
            }

        }
        #endregion
    }



}
