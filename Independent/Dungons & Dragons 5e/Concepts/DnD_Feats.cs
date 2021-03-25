using System;

namespace Dungeons_and_Dragons
{
    public enum Feat 
    {
        Alert = 0,
        Athlete = 1,
        Actor = 2,
        Charger =3,
        CrossbowExpert =4,
        DefensiveDuelist=5,
        DualWielder=6,
        DungeonDelver=7,
        Durable=8,
        ElementalAdept=9,
        Grappler=10,
        GreatWeaponMaster=11,
        Healer=12,
        HeavilyArmored=13,
        HeavyArmorMaster=14,
        InspiringLeader=15,
        KeenMind=16,
        LightlyArmored=17,
        Linguist=18, 
        Lucky=19, 
        MageSlayer=20,
        MagicInitiate=21,
        MertialAdept=22,
        MediumArmorMaster=23,
        Mobile=24,
        ModeratelyArmored=25,
        MountedCombatant=26,
        Observant=27,
        PolearmMaster=28,
        Resilent=29,
        RitualCaster=30,
        SavageAttacker=31,
        Sentinel=32, 
        Sharpshooter=33,
        ShieldMaster=34, 
        Skilled=35,
        Skulker=36, 
        SpellSniper=37,
        TavernBrawler=38,
        Tough=39,
        WarCaster=40,
        WeaponMaster=41
    }

    [Serializable]
    public class FeatsSet : QuizCanners.Utils.SerializableHashSetForEnum<Feat> { }
}
