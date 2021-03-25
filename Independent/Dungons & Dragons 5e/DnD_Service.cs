using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [ExecuteAlways]
    public class DnD_Service : Singleton.BehaniourBase
    {
        public DnDPrototypesScriptableObject DnDPrototypes;

        public SeededFallacks Fallbacks => DnDPrototypes ? DnDPrototypes.Fallbacks : null;

        #region Inspector

        public override string InspectedCategory => "";

        public override string NeedAttention()
        {
            if (!DnDPrototypes)
                return "No Prototypes";

            return base.NeedAttention();
        }

        public override string GetReadOnlyName() => "Dungeons & Dragons 5e";

        public override void Inspect()
        {
            base.Inspect();

            if (!DnDPrototypes)
                "Prototypes".PegiLabel().edit(ref DnDPrototypes).nl();
            else
                DnDPrototypes.Nested_Inspect();
        }

        #endregion
    }
    

  [PEGI_Inspector_Override(typeof(DnD_Service))] internal class DnD_ManagerDrawer : PEGI_Inspector_Override { }

}