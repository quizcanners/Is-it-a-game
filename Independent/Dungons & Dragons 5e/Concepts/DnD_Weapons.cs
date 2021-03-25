using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{

    public class Weapon : IGotName, ISearchable
    {
        private string _name;
        public Dice Damage { get; private set; }

        public Wallet wallet = new Wallet();

        public DamageType DamageType { get; private set; }
        public bool IsRanged { get; private set; }
        public bool IsMartial { get; private set; }

        private Property _properties;
        public bool this[Property prop] =>  _properties.HasFlag(prop);

        public string NameForInspector { get => _name; set => _name = value; }

        [Flags]
        public enum Property 
        {
            None = 0,
            Ammunition = 1,
            Finesse = 2,
            Heavy = 4,
            Light = 8,
            Loading = 16,
            Range = 32,
            Reach = 64,
            Special = 128,
            Thrown = 256,
            TwoHanded = 512,
            Versetile = 1024,
        }


        public Weapon(string name, Dice dice, DamageType damageType, Property properties = Property.None, bool isRanged = false, bool isMartial = false)
        {
            Damage = dice;
            DamageType = damageType;
            _properties = properties;
            IsRanged = isRanged;
            IsMartial = isMartial;
            NameForInspector = QcSharp.AddSpacesToSentence(name);
        }

        private Weapon Price(Currency currency, int amount) 
        {
            wallet[currency] += amount;
            return this;
        }

        public IEnumerator<object> SearchKeywordsEnumerator()
        {
            yield return DamageType.ToString();

            foreach (Property v in Enum.GetValues(typeof(Property)))
                if (_properties.HasFlag(v))
                    yield return v.ToString();
        }


        #region SmartId

        private static StaticDictionaryGeneric<Weapon> _allElementsContainer = new StaticDictionaryGeneric<Weapon>(
                new Weapon("Club", dice: Dice.D4, DamageType.Bludgeoning, Property.Light).Price(Currency.Silver, 1),
                new Weapon("Dagger", dice: Dice.D4, DamageType.Piercing, Property.Finesse | Property.Light | Property.Thrown).Price(Currency.Gold, 2),
                new Weapon("Greatclub", dice: Dice.D8, DamageType.Bludgeoning, Property.TwoHanded).Price(Currency.Silver, 2),
                new Weapon("Handaxe", dice: Dice.D6, DamageType.Slashing).Price(Currency.Gold, 5),
                new Weapon("Javelin", dice: Dice.D6, DamageType.Piercing).Price(Currency.Silver, 5),
                new Weapon("LightHammer", dice: Dice.D4, DamageType.Bludgeoning).Price(Currency.Gold, 2),
                new Weapon("Mace", dice: Dice.D6, DamageType.Bludgeoning).Price(Currency.Gold, 5),
                new Weapon("Quarterstaff", dice: Dice.D6, DamageType.Bludgeoning).Price(Currency.Silver, 2),
                new Weapon("Sickle", dice: Dice.D4, DamageType.Slashing).Price(Currency.Gold, 1),
                new Weapon("Spear", dice: Dice.D6, DamageType.Piercing).Price(Currency.Gold, 1),


                new Weapon("Crossbow, light", dice: Dice.D8, DamageType.Piercing).Price(Currency.Gold, 25),
                new Weapon("Dart", dice: Dice.D4, DamageType.Piercing).Price(Currency.Copper, 5),
                new Weapon("Shortbow", dice: Dice.D6, DamageType.Piercing).Price(Currency.Gold, 25),
                new Weapon("Sling", dice: Dice.D4, DamageType.Bludgeoning).Price(Currency.Silver, 1),

                new Weapon("Battleaxe", dice: Dice.D8, DamageType.Slashing).Price(Currency.Gold, 10),
                new Weapon("Flail", dice: Dice.D8, DamageType.Bludgeoning).Price(Currency.Gold, 10),
                new Weapon("Glaive", dice: Dice.D10, DamageType.Slashing).Price(Currency.Gold, 20),
                new Weapon("Greataxe", dice: Dice.D12, DamageType.Slashing).Price(Currency.Gold, 30),
                new Weapon("Greatsword", dice: Dice.D12, DamageType.Slashing).Price(Currency.Gold, 50), // 2d6
                new Weapon("Halberd", dice: Dice.D10, DamageType.Slashing).Price(Currency.Gold, 20),
                new Weapon("Lance", dice: Dice.D12, DamageType.Piercing).Price(Currency.Gold, 10),
                new Weapon("Longsword", dice: Dice.D8, DamageType.Slashing).Price(Currency.Gold, 15),
                new Weapon("Maul", dice: Dice.D12, DamageType.Bludgeoning).Price(Currency.Gold, 10), // 2d6
                new Weapon("Morningstar", dice: Dice.D8, DamageType.Piercing).Price(Currency.Gold, 15),
                new Weapon("Pike", dice: Dice.D10, DamageType.Piercing).Price(Currency.Gold, 5),
                new Weapon("Rapier", dice: Dice.D8, DamageType.Piercing).Price(Currency.Gold, 25),
                new Weapon("Scimitar", dice: Dice.D6, DamageType.Slashing).Price(Currency.Gold, 25),
                new Weapon("Shortsword", dice: Dice.D6, DamageType.Piercing).Price(Currency.Gold, 5),
                new Weapon("Trident", dice: Dice.D6, DamageType.Piercing).Price(Currency.Gold, 5),
                new Weapon("War pick", dice: Dice.D8, DamageType.Piercing).Price(Currency.Gold, 5),
                new Weapon("Warhammer", dice: Dice.D8, DamageType.Bludgeoning).Price(Currency.Gold, 15),
                new Weapon("Whip", dice: Dice.D4, DamageType.Slashing).Price(Currency.Gold, 2),


                new Weapon("Blowgun", dice: Dice.D2, DamageType.Piercing, isMartial: true).Price(Currency.Gold, 10),
                new Weapon("Crossbow, hand", dice: Dice.D6, DamageType.Piercing, isMartial: true).Price(Currency.Gold, 75),
                new Weapon("Crossbow, heavy", dice: Dice.D10, DamageType.Piercing, isMartial: true).Price(Currency.Gold, 50),
                new Weapon("Longbow", dice: Dice.D8, DamageType.Piercing, isMartial: true).Price(Currency.Gold, 50),
                new Weapon("Net", dice: Dice.D2, DamageType.Piercing, isMartial: true).Price(Currency.Gold, 1)

            );
    
        [Serializable]
        public class SmartId : SmartStringIdGeneric<Weapon>
        {
            protected override Dictionary<string, Weapon> GetEnities() => _allElementsContainer.AllElements;

            public SmartId() 
            {
                Id = "Dagger";
            }
        }
        #endregion
    }

    
}