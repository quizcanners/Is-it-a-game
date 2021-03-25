using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers
{
    [ExecuteAlways]
    public class TriggerValuesService : Singleton.BehaniourBase, IPEGI
    {
        public const string TRIGGERS = "Triggers";

        [SerializeField] internal TriggerGroupsDictionary triggerGroup = new();
        public int Version;
        public TriggerValues Values = new();

        internal TriggerGroupMeta.TriggerMeta this[IIntTriggerIndex index]
        {
            get
            {
                if (!index.IsValid())
                    return null;

                if (triggerGroup.TryGetValue(index.GetGroupId(), out var group))
                    return group[index];

                return null;
            }
        }

        internal TriggerGroupMeta.TriggerMeta this[IBoolTriggerIndex index]
        {
            get
            {
                if (!index.IsValid())
                    return null;

                if (triggerGroup.TryGetValue(index.GetGroupId(), out var group))
                    return group[index];

                return null;
            }
        }

        #region Inspector

        public override string InspectedCategory => Utils.Singleton.Categories.GAME_LOGIC;

        private int _inspectedStuff = -1;
        private int _inspectedGroup = -1;
        public override void Inspect()
        {
            pegi.nl();
            "Values".PegiLabel().enter_Inspect(Values, ref _inspectedStuff, 0).nl();
            if ("Trigger Groups".PegiLabel().isEntered(ref _inspectedStuff, 1).nl())
               pegi.edit_Dictionary(triggerGroup,  ref _inspectedGroup).nl();
        }

        #endregion


        [Serializable]
        internal class TriggerGroupsDictionary : SerializableDictionary<string, TriggerGroupMeta> { }

    }

    [PEGI_Inspector_Override(typeof(TriggerValuesService))] internal class TriggerValuesServiceDrawer : PEGI_Inspector_Override { }
}
