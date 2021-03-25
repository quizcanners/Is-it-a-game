using System.Collections.Generic;
using QuizCanners.Migration;

namespace QuizCanners.IsItGame.Triggers
{

    public enum ResultType
    {
        Set, Add, Subtract 
    }
    
    public static class ResultExtensionFunctions 
    {
        public static string GetText(this ResultType type) {
            return type switch
            {
                ResultType.Set => "=",
                ResultType.Add => "+",
                ResultType.Subtract => "-",
                _ => type.ToString(),
            };
        }

        internal static void Apply(this List<IntResult> results) {
            
            if (results.Count <= 0) return;
            
            foreach (var r in results)
                r.Apply();
        }
    }

    internal class IntResult : TriggerIndexBase, IIntTriggerIndex
    {
        public ResultType type;
        public int updateValue;

        public override bool IsBooleanValue => false;

        public void Apply()
        {
            switch (type)
            {
                case ResultType.Set: Values[this] = updateValue; break;
                case ResultType.Add: Values[this] += updateValue; break;
                case ResultType.Subtract: Values[this] -= updateValue; break;
            }
        }

        public int GetValue() => Values[this];

        #region Encode & Decode
        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "ty": type = (ResultType)data.ToInt(); break;
                case "val": updateValue = data.ToInt(); break;
                case "ind": data.ToDelegate(DecodeIndex); break;
            }
        }

        public override CfgEncoder Encode() => new CfgEncoder()
                .Add_IfNotZero("ty", (int)type)
                .Add_IfNotZero("val", updateValue)
                .Add("ind", EncodeIndex);
        #endregion
    }
}