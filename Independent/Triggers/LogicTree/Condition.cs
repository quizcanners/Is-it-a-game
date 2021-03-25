using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Triggers
{
    public interface IAmConditional {
        bool CheckConditions();
    }
    
    public enum ConditionType 
    {
        Above, Below, Equals, NotEquals 
    }

    [DerivedList(typeof(ConditionBool), typeof(ConditionInt))]
    internal abstract class ConditionBase : TriggerIndexBase
    {
        public virtual bool TryForceConditionValue(bool toTrue) => false;

        public virtual bool IsConditionTrue() => false;
    }

    internal class ConditionBool : ConditionBase, IBoolTriggerIndex {

        public bool compareValue;

        public override bool IsBooleanValue => true;

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfTrue("b", compareValue)
            .Add("ind", EncodeIndex());

        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "b": compareValue = data.ToBool(); break;
                case "ind": data.ToDelegate(DecodeIndex); break;
            }
        }
        #endregion

        public override bool TryForceConditionValue(bool toTrue)
        {
            Values[this] = toTrue ? compareValue : !compareValue;
            return true;
        }

        public override bool IsConditionTrue() => GetValue() == compareValue;

        public bool GetValue() => Values[this];
    }

    internal class ConditionInt : ConditionBase, IIntTriggerIndex 
    {
        public ConditionType type;
        public int compareValue;

        public override bool IsBooleanValue => false;

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotZero("v", compareValue)
            .Add_IfNotZero("ty", (int)type)
            .Add("ind", EncodeIndex);

        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "v": compareValue = data.ToInt(); break;
                case "ty": type = (ConditionType)data.ToInt(); break;
                case "ind": data.ToDelegate(DecodeIndex); break;
            }
        }
        #endregion

        #region Inspect

        public override string GetReadOnlyName() {
                var name = "If {0} {1} ".F(base.GetReadOnlyName(), type.GetName()//, Trigger.Usage.GetConditionValueName(this)
                    );
                return name;
        }

        #endregion

        public override bool TryForceConditionValue(bool toTrue) {

            if (IsConditionTrue() == toTrue)
                return true;
            
            if (toTrue) 
            {
                switch (type) 
                { 
                    case ConditionType.Above:                   Values[this] = compareValue + 1;                                                break;
                    case ConditionType.Below:                   Values[this] = compareValue - 1;                                                break;
                    case ConditionType.Equals:                  Values[this] = compareValue;                                                    break;
                    case ConditionType.NotEquals:               if (Values[this] == compareValue) Values[this] = 1;                             break;
                }
            } else 
            {
                switch (type) 
                {
                    case ConditionType.Above: Values[this] = compareValue - 1;   break;
                    case ConditionType.Below: Values[this] = compareValue + 1;   break;
                    case ConditionType.Equals: Values[this] = compareValue + 1;  break;
                    case ConditionType.NotEquals: Values[this] = compareValue;   break;
                }
            }

            return true;
        }

        public override bool IsConditionTrue()
        {
            switch (type)
            {
                case ConditionType.Above:                   if (GetValue() > compareValue) return true;                     break;
                case ConditionType.Below:                   if (GetValue() < compareValue) return true;                     break;
                case ConditionType.Equals:                  if (GetValue() == compareValue) return true;                    break;
                case ConditionType.NotEquals:               if (GetValue() != compareValue) return true;                    break;
            }
            return false;
        }

        public int GetValue() => Values[this];
    }
    
    public static class ConditionExtensions
    {
        public static bool Is_All_ConditionsTrue(this List<IAmConditional> lst)
        {
            if (lst.IsNullOrEmpty())
                return true;

            foreach (var e in lst)
                if (e != null && !e.CheckConditions())
                    return false;
            return true;
        }

        public static bool IsTrue(this IAmConditional cond) => cond.CheckConditions();

        public static string GetName(this ConditionType type)
        {
            return type switch
            {
                ConditionType.Equals => "==",
                ConditionType.Above => ">",
                ConditionType.Below => "<",
                ConditionType.NotEquals => "!=",
                _ => "!!!Unrecognized Condition Type",
            };
        }
    }
}