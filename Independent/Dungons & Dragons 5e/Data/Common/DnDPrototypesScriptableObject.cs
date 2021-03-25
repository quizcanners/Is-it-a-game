using Dungeons_and_Dragons.Calculators;
using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/Dungeons & Dragons/" + FILE_NAME)]
    public class DnDPrototypesScriptableObject : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Prototypes of Dungeons & Dragons";

        public CharactersDictionary Characters = new();
        public MonstersDictionary Monsters = new();
        public RandomElementsRollTablesDictionary RollTables = new();
        public EncounterCalculator EncounterCalculator = new();
        public CombatTracker CombatTracker = new();
        public AvarageDamageCalculator DamageCalculator = new();
        public DiceCalculator DiceCalculator = new();

        public SeededFallacks Fallbacks;

        #region Inspector

        [SerializeField] private pegi.CollectionInspectorMeta _characterListMeta =   new("Characters",     showCopyPasteOptions: true);
        [SerializeField] private pegi.CollectionInspectorMeta _monsterListMeta =     new("Monsters",       showCopyPasteOptions: true);
        [SerializeField] private pegi.CollectionInspectorMeta _rollTablesListMeta =  new("Roll Tables",    showCopyPasteOptions: true);

        protected pegi.EnterExitContext context = new();
        protected pegi.EnterExitContext _calculators = new();

        public void Inspect()
        {
            if (!Singleton.Get <DnD_Service>()) 
            {
                "Instance is Null. Have {0} in the scene to initialize internal singleton".F(nameof(DnD_Service)).PegiLabel().writeWarning();
                return;
            }

            pegi.nl();

            using (context.StartContext())
            {
                _characterListMeta.enter_Dictionary(Characters).nl();
                _monsterListMeta.enter_Dictionary(Monsters).nl();
                _rollTablesListMeta.enter_Dictionary(RollTables).nl();
                "Seeded Fallbacks".PegiLabel().enter_Inspect(Fallbacks).nl();

                if ("Calculators".PegiLabel().isEntered().nl())
                {
                    using (_calculators.StartContext())
                    {
                        "Encounter Calculator".PegiLabel().enter_Inspect(EncounterCalculator).nl();
                        "Combat Tracker".PegiLabel().enter_Inspect(CombatTracker).nl();
                        "Damage Calculator".PegiLabel().enter_Inspect(DamageCalculator).nl();
                        "Dice Calculator".PegiLabel().enter_Inspect(DiceCalculator).nl();
                    }
                }
            }

        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(DnDPrototypesScriptableObject))] 
    internal class DungeonsAndDragonsCharactersScriptableObjectDrawer : PEGI_Inspector_Override { }
}