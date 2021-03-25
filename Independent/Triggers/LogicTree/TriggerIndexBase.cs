using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using static QuizCanners.IsItGame.Triggers.TriggerValuesService;

namespace QuizCanners.IsItGame.Triggers
{
    internal interface ITriggerIndex 
    {
        public string GetGroupId(); // { get; }
        public string GetTriggerId(); // { get; }
        public bool IsBooleanValue { get; }
        public bool IsValid();
    }

    internal interface IIntTriggerIndex : ITriggerIndex
    {
        public int GetValue();
    }

    internal interface IBoolTriggerIndex : ITriggerIndex
    {
        public bool GetValue();
    }

    internal abstract class TriggerIndexBase : ITriggerIndex, ICfg, IGotReadOnlyName, IPEGI_ListInspect, IPEGI {

        public string GroupId;
        public string TriggerId;
        public abstract bool IsBooleanValue { get; }

        public string GetGroupId() => GroupId;
        public string GetTriggerId() => TriggerId;
        public bool IsValid() => GroupId.IsNullOrEmpty() == false && TriggerId.IsNullOrEmpty() == false;

        public TriggerIndexBase TriggerIndexes 
        { 
            set 
            { 
                if (value != null) 
                {
                    GroupId = value.GroupId;
                    TriggerId = value.TriggerId;
                }
            }
        }
        
        #region Encode & Decode
        public abstract CfgEncoder Encode();
        public abstract void DecodeTag(string tg, CfgData data);

        protected CfgEncoder EncodeIndex() => new CfgEncoder()
            .Add_String("gi", GroupId)
            .Add_String("ti", TriggerId);

        protected void DecodeIndex(string tag, CfgData data)
        {
            switch (tag)
            {
                case "gi": GroupId = data.ToString(); break;
                case "ti": TriggerId = data.ToString(); break;
            }
        }
        #endregion

        protected TriggerValues Values => Singleton.Get<TriggerValuesService>().Values;

        protected TriggerGroupsDictionary Groups => Singleton.Get<TriggerValuesService>().triggerGroup;

        #region Inspector

        public virtual string GetReadOnlyName() => GetType().ToPegiStringType();

        public void InspectInList(ref int edited, int index)
        {
            InspectSelect();

            if (icon.Enter.Click())
                edited = index;
        }

        private void InspectSelect() 
        {
            if (Values == null)
            {
                "No Values".PegiLabel().write();
            }
            else
            {
                "Group".PegiLabel(width: 60).select(ref GroupId, Groups);
                if (GroupId.IsNullOrEmpty() == false)
                {
                    if (Groups.TryGetValue(GroupId, out TriggerGroupMeta gr))
                    {
                        "Trig".PegiLabel(50).select(ref TriggerId, gr.GetDictionary(this));
                    }
                }
            }
        }

        public virtual void Inspect()
        {
            InspectSelect();

            Singleton.Try<TriggerValuesService>(s => s.Nested_Inspect());
        }

        #endregion
    }
}