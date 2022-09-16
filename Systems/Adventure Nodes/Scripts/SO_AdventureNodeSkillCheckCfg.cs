using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.Triggers;
using QuizCanners.Utils;
using QuizCanners.Migration;
using System;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class SO_AdventureNodeSkillCheckCfg : CfgSelfSerializationBase, IPEGI, IGotName, IPEGI_ListInspect
    {
        public string Name;
        public AbilityCheck Check = new();
        public ConditionBranch Condition = new();

        public string NameForInspector 
        { 
            get => Name; 
            set => Name = value; 
        }

        public override void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                case "c": data.DecodeOverride(ref Condition); break;
            }
        }

        public override CfgEncoder Encode() => new CfgEncoder().Add("c", Condition);
        
        public void Inspect()
        {
            "Name".PegiLabel(60).Edit(ref Name).Nl();
            Check.Nested_Inspect().Nl();
            pegi.Line();
            Condition.Nested_Inspect().Nl();
        }

        public void InspectInList(ref int edited, int index)
        {
            if ("{0} ".F(Name, Check).PegiLabel().ClickLabel() | Icon.Enter.Click())
                edited = index;
        }
    }
}
