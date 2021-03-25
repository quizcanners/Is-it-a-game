using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{

    public abstract class RandomElementsRollTables : ScriptableObject, IGotName, IPEGI_ListInspect
    {
        protected const string TABLE_CREATE_NEW_PATH = "Quiz Canners/Dungeons & Dragons/Roll Table/";

        public virtual void UpdatePrototypes() { }

        public abstract string GetRolledElementName(RolledTable.Result result, bool shortText);

        public abstract string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText);


        public virtual bool TryGetConcept<CT>(out CT value, RanDndSeed seed, IConceptValueProvider proider)
        {
            value = default;
            return false;
        }

        public virtual bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT : IComparable
        {
            value = default;
            return false;
        }

        public void Roll(RolledTable.Result result) 
        {
             RollInternal(result);
        }

        protected abstract void RollInternal(RolledTable.Result result);

        #region Inspector
        public string NameForInspector 
        { 
            get => name;
            set => QcUnity.RenameAsset(this, value); 
        }

        public virtual void Inspect(RolledTable.Result result) 
        {
        }

        internal virtual void SelectInternal(RolledTable.Result result) { }

        public void InspectInList(ref int edited, int index, RolledTable.Result result) 
        {
      
            InspectInList_Internal(ref edited, index, result);

            SelectInternal(result);

            icon.Dice.Click(()=> Roll(result));

            this.ClickHighlight();
            
        }

        protected virtual void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() | "{0} | {1} : {2}".F(index, this.GetNameForInspector().Replace("Random ", ""), GetRolledElementName(result, shortText: true)).PegiLabel().ClickLabel())
                edited = index;
        }

        public void InspectInList(ref int edited, int index)
        {
            if (icon.Enter.Click() | "{0} | {1}".F(index, this.GetNameForInspector().Replace("Random ", "")).PegiLabel().ClickLabel())
                edited = index;

            this.ClickHighlight();
        }

        #endregion
    }

    [Serializable]
    public class RandomElementsRollTablesDictionary : SerializableDictionary<string, RandomElementsRollTables> { }
}
