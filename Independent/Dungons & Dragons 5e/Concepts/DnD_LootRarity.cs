namespace Dungeons_and_Dragons
{

    public static class DnDLootExtensions 
    {
        public static int Bonus(this LootRarity loot) => ((int)loot);

    }

    public enum LootRarity 
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        VeryRare = 3,
        Legendary = 4,



    }

}